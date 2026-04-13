using System.Text.Json;
using System.Text.RegularExpressions;
using Europiyum.Cms.Application.Public.Models;
using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Domain.Enums;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Europiyum.Cms.Application.Services;

public class PublicContentService : IPublicContentService
{
    private static readonly Regex SafeRelativeStaticPathRegex = new("^[a-zA-Z0-9_./-]+$", RegexOptions.Compiled);

    private readonly CmsDbContext _db;
    private readonly ILogger<PublicContentService> _logger;

    public PublicContentService(CmsDbContext db, ILogger<PublicContentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PublicHomeViewModel?> GetHomeAsync(string companyCode, string? languageCode, CancellationToken cancellationToken = default)
    {
        var resolved = await PublicSiteContextResolver.TryResolveAsync(_db, companyCode, languageCode, cancellationToken);
        if (resolved is null)
        {
            _logger.LogWarning("Company not found for code {Code}", companyCode);
            return null;
        }

        var (company, language, langCode) = resolved;

        var heroRow = await _db.HomePageSections.AsNoTracking()
            .Where(s => s.CompanyId == company.Id && s.SectionKey == "hero" && s.IsActive)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Id)
            .Select(s => new { s.Id, s.LinkedComponentItemId })
            .FirstOrDefaultAsync(cancellationToken);

        string? title = null;
        string? subtitle = null;
        string? heroBody = null;
        if (heroRow is not null)
        {
            var tr = await _db.HomePageSectionTranslations.AsNoTracking()
                .FirstOrDefaultAsync(t => t.HomePageSectionId == heroRow.Id && t.LanguageId == language.Id, cancellationToken);
            if (tr is null)
            {
                var defLangId = company.DefaultLanguageId;
                tr = await _db.HomePageSectionTranslations.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.HomePageSectionId == heroRow.Id && t.LanguageId == defLangId, cancellationToken);
            }

            title = tr?.Title;
            subtitle = tr?.Subtitle;
            heroBody = tr?.BodyHtml;

            if (string.IsNullOrWhiteSpace(heroBody) && heroRow.LinkedComponentItemId is int heroCompId)
            {
                var comp = await _db.ComponentItems.AsNoTracking()
                    .Where(c => c.Id == heroCompId && c.CompanyId == company.Id && c.IsActive)
                    .Include(c => c.Translations)
                    .FirstOrDefaultAsync(cancellationToken);
                if (comp is not null)
                {
                    var ctr = PickComponentTranslation(comp.Translations, language.Id, company.DefaultLanguageId);
                    heroBody = ctr?.BodyHtml;
                }
            }
        }

        var vm = new PublicHomeViewModel
        {
            CompanyName = company.Name,
            CompanyCode = company.Code,
            RazorViewName = MapHomepageKeyToViewName(company.HomepageVariantKey),
            LanguageCode = langCode,
            HeroTitle = title ?? company.Name,
            HeroSubtitle = subtitle,
            HeroBodyHtml = string.IsNullOrWhiteSpace(heroBody) ? null : heroBody.Trim()
        };

        var homePageId = await _db.Pages.AsNoTracking()
            .Where(p => p.CompanyId == company.Id && p.PageType == PageType.Home && p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Id)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (homePageId is not null)
        {
            var seo = await _db.SeoMetadata.AsNoTracking()
                .FirstOrDefaultAsync(
                    s => s.PageId == homePageId && s.LanguageId == language.Id,
                    cancellationToken);
            if (seo is not null)
            {
                vm.MetaTitle = seo.MetaTitle;
                vm.MetaDescription = seo.MetaDescription;
                vm.MetaKeywords = seo.MetaKeywords;
                vm.CanonicalUrl = seo.CanonicalUrl;
                vm.Robots = seo.Robots;
            }
        }

        var contentSections = await _db.HomePageSections.AsNoTracking()
            .Where(s => s.CompanyId == company.Id && s.IsActive && s.SectionKey != "hero")
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Id)
            .Include(s => s.Translations)
            .ToListAsync(cancellationToken);

        var linkedIds = contentSections
            .Where(s => s.LinkedComponentItemId is not null)
            .Select(s => s.LinkedComponentItemId!.Value)
            .Distinct()
            .ToList();

        var linkedComponents = new Dictionary<int, ComponentItem>();
        if (linkedIds.Count > 0)
        {
            var items = await _db.ComponentItems.AsNoTracking()
                .Where(c => linkedIds.Contains(c.Id) && c.CompanyId == company.Id && c.IsActive)
                .Include(c => c.Translations)
                .ToListAsync(cancellationToken);
            foreach (var c in items)
                linkedComponents[c.Id] = c;
        }

        var blocks = new List<PublicHomeSectionBlock>();
        foreach (var s in contentSections)
        {
            var tr = PickHomeSectionTranslation(s.Translations, language.Id, company.DefaultLanguageId);
            var html = tr?.BodyHtml;
            if (string.IsNullOrWhiteSpace(html)
                && s.LinkedComponentItemId is int lid
                && linkedComponents.TryGetValue(lid, out var comp))
            {
                var ctr = PickComponentTranslation(comp.Translations, language.Id, company.DefaultLanguageId);
                html = ctr?.BodyHtml;
            }

            if (string.IsNullOrWhiteSpace(html))
                continue;

            blocks.Add(new PublicHomeSectionBlock { SectionKey = s.SectionKey, BodyHtml = html });
        }

        if (homePageId is not null)
        {
            var pageComps = await _db.PageComponents.AsNoTracking()
                .Where(pc => pc.PageId == homePageId.Value)
                .OrderBy(pc => pc.SortOrder)
                .ThenBy(pc => pc.Id)
                .Include(pc => pc.ComponentItem)
                    .ThenInclude(ci => ci.ComponentType)
                .Include(pc => pc.ComponentItem)
                    .ThenInclude(ci => ci.Translations)
                .ToListAsync(cancellationToken);

            foreach (var pc in pageComps)
            {
                var item = pc.ComponentItem;
                if (!item.IsActive)
                    continue;

                var ctr = PickComponentTranslation(item.Translations, language.Id, company.DefaultLanguageId);
                var html = ctr?.BodyHtml;
                if (string.IsNullOrWhiteSpace(html))
                    continue;

                var typeKey = item.ComponentType.Key;
                if (string.IsNullOrWhiteSpace(typeKey))
                    typeKey = "component";

                blocks.Add(new PublicHomeSectionBlock { SectionKey = typeKey, BodyHtml = html });
            }
        }

        vm.Sections = blocks;

        return vm;
    }

    public async Task<PublicPageViewModel?> GetPageBySlugAsync(
        string companyCode,
        string slug,
        string? languageCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        var resolved = await PublicSiteContextResolver.TryResolveAsync(_db, companyCode, languageCode, cancellationToken);
        if (resolved is null)
        {
            _logger.LogWarning("Company not found for page slug {Slug}", slug);
            return null;
        }

        var (company, language, langCode) = resolved;
        var slugNorm = slug.Trim();

        var pageId = await _db.Pages.AsNoTracking()
            .Where(p => p.CompanyId == company.Id && p.IsActive)
            .Where(p => p.Slug == slugNorm || p.Translations.Any(t => t.Slug == slugNorm))
            .OrderBy(p => p.SortOrder)
            .Select(p => p.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (pageId == 0)
            return null;

        var page = await _db.Pages.AsNoTracking()
            .Include(p => p.Translations)
            .FirstAsync(p => p.Id == pageId, cancellationToken);

        var tr = page.Translations.FirstOrDefault(t => t.LanguageId == language.Id)
            ?? page.Translations.FirstOrDefault(t => t.LanguageId == company.DefaultLanguageId)
            ?? page.Translations.FirstOrDefault();

        var seo = await _db.SeoMetadata.AsNoTracking()
            .FirstOrDefaultAsync(s => s.PageId == page.Id && s.LanguageId == language.Id, cancellationToken);

        PublicPageFormVm? formVm = null;
        if (page.FormDefinitionId is { } fDefId)
        {
            var fd = await _db.FormDefinitions.AsNoTracking()
                .Include(f => f.Fields)
                .FirstOrDefaultAsync(
                    f => f.Id == fDefId && f.CompanyId == company.Id && f.IsActive,
                    cancellationToken);
            if (fd is not null)
            {
                formVm = new PublicPageFormVm
                {
                    Key = fd.Key,
                    Name = fd.Name,
                    Fields = fd.Fields
                        .OrderBy(x => x.SortOrder)
                        .ThenBy(x => x.Id)
                        .Select(x => new PublicPageFormFieldVm
                        {
                            FieldKey = x.FieldKey,
                            FieldType = x.FieldType,
                            Label = x.DefaultLabel,
                            IsRequired = x.IsRequired,
                            Options = ParseFieldOptions(x.OptionsJson)
                        })
                        .ToList()
                };
            }
        }

        return new PublicPageViewModel
        {
            CompanyName = company.Name,
            LanguageCode = langCode,
            Title = tr?.Title ?? page.Slug,
            HtmlContent = tr?.HtmlContent,
            MetaTitle = seo?.MetaTitle,
            MetaDescription = seo?.MetaDescription,
            MetaKeywords = seo?.MetaKeywords,
            CanonicalUrl = seo?.CanonicalUrl,
            Robots = seo?.Robots,
            BreadcrumbHeading = string.IsNullOrWhiteSpace(tr?.BreadcrumbHeading) ? null : tr.BreadcrumbHeading.Trim(),
            BreadcrumbBackgroundRelativePath = SanitizeBreadcrumbBackgroundPath(tr?.BreadcrumbBackgroundPath),
            Form = formVm
        };
    }

    private static List<string> ParseFieldOptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<string>();
        try
        {
            var list = JsonSerializer.Deserialize<List<string>>(json);
            if (list is null)
                return new List<string>();
            return list
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToList();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }

    private static string SanitizeBreadcrumbBackgroundPath(string? path)
    {
        const string fallback = "images/banner/banner-inner.jpg";
        if (string.IsNullOrWhiteSpace(path))
            return fallback;

        var p = path.Trim().Replace('\\', '/').TrimStart('/');
        if (p.Contains("..", StringComparison.Ordinal) || p.Contains("://", StringComparison.Ordinal))
            return fallback;

        return SafeRelativeStaticPathRegex.IsMatch(p) ? p : fallback;
    }

    /// <summary>
    /// Maps DB key (e.g. index-3-dark) to Razor view name (Index3Dark).
    /// </summary>
    private static string MapHomepageKeyToViewName(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return "Index";

        var parts = key.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            if (parts[i].Equals("index", StringComparison.OrdinalIgnoreCase))
                parts[i] = "Index";
            else
                parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i][1..].ToLowerInvariant();
        }

        return string.Concat(parts);
    }

    private static HomePageSectionTranslation? PickHomeSectionTranslation(
        IEnumerable<HomePageSectionTranslation> translations,
        int languageId,
        int defaultLanguageId)
    {
        return translations.FirstOrDefault(t => t.LanguageId == languageId)
            ?? translations.FirstOrDefault(t => t.LanguageId == defaultLanguageId)
            ?? translations.FirstOrDefault();
    }

    private static ComponentTranslation? PickComponentTranslation(
        IEnumerable<ComponentTranslation> translations,
        int languageId,
        int defaultLanguageId)
    {
        return translations.FirstOrDefault(t => t.LanguageId == languageId)
            ?? translations.FirstOrDefault(t => t.LanguageId == defaultLanguageId)
            ?? translations.FirstOrDefault();
    }
}
