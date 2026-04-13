using Europiyum.Cms.Application.Public.Models;
using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Domain.Enums;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Europiyum.Cms.Application.Services;

public class PublicMenuService : IPublicMenuService
{
    private readonly CmsDbContext _db;
    private readonly IPublicAppearanceService _appearance;
    private readonly ILogger<PublicMenuService> _logger;

    public PublicMenuService(
        CmsDbContext db,
        IPublicAppearanceService appearance,
        ILogger<PublicMenuService> logger)
    {
        _db = db;
        _appearance = appearance;
        _logger = logger;
    }

    public async Task<SiteHeaderViewModel> BuildHeaderAsync(
        string companyCode,
        string siteTitle,
        string? languageCode,
        CancellationToken cancellationToken = default)
    {
        var resolved = await PublicSiteContextResolver.TryResolveAsync(_db, companyCode, languageCode, cancellationToken);
        if (resolved is null)
        {
            _logger.LogWarning("Company not found for menu build: {Code}", companyCode);
            return new SiteHeaderViewModel
            {
                SiteTitle = siteTitle,
                LanguageCode = string.IsNullOrWhiteSpace(languageCode) ? "tr" : languageCode.Trim(),
                NavLinks = Array.Empty<PublicNavLinkVm>()
            };
        }

        var (company, language, langCode) = resolved;

        var menuId = await _db.Menus.AsNoTracking()
            .Where(m => m.CompanyId == company.Id && m.Kind == MenuKind.Header)
            .OrderBy(m => m.Id)
            .Select(m => (int?)m.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (menuId is null)
        {
            return new SiteHeaderViewModel
            {
                SiteTitle = siteTitle,
                LanguageCode = langCode,
                NavLinks = Array.Empty<PublicNavLinkVm>()
            };
        }

        var items = await _db.MenuItems.AsNoTracking()
            .Where(i => i.MenuId == menuId && i.IsActive)
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Id)
            .Include(i => i.Translations)
            .ToListAsync(cancellationToken);

        var pageIds = items
            .Where(i => i.TargetPageId is not null)
            .Select(i => i.TargetPageId!.Value)
            .Distinct()
            .ToList();

        var slugByPageId = await LoadSlugMapAsync(pageIds, language.Id, cancellationToken);

        var byParent = items.ToLookup(i => i.ParentMenuItemId);
        var roots = byParent[null].ToList();

        var nav = roots
            .Select(i => MapItem(i, byParent, slugByPageId, language, langCode))
            .ToList();

        return new SiteHeaderViewModel
        {
            SiteTitle = siteTitle,
            LanguageCode = langCode,
            NavLinks = nav
        };
    }

    public async Task<SiteFooterViewModel> BuildFooterAsync(
        string companyCode,
        string siteTitle,
        string? languageCode,
        CancellationToken cancellationToken = default)
    {
        var resolved = await PublicSiteContextResolver.TryResolveAsync(_db, companyCode, languageCode, cancellationToken);
        if (resolved is null)
        {
            _logger.LogWarning("Company not found for footer build: {Code}", companyCode);
            return new SiteFooterViewModel
            {
                SiteTitle = siteTitle,
                LanguageCode = string.IsNullOrWhiteSpace(languageCode) ? "tr" : languageCode.Trim()
            };
        }

        var (company, _, langCode) = resolved;
        var appearance = await _appearance.GetSnapshotAsync(company.Code, cancellationToken);

        return new SiteFooterViewModel
        {
            SiteTitle = siteTitle,
            LanguageCode = langCode,
            FooterLogoSrc = appearance.FooterLogoHref,
            IntroHtml = appearance.FooterIntroHtml,
            BodyHtml = appearance.FooterBodyHtml,
            CopyrightHtml = appearance.FooterCopyrightHtml,
            Columns = new List<FooterColumnVm>(),
            BottomLinks = new List<FooterLinkVm>(),
            ContactEmail = appearance.ContactEmail,
            ContactPhone = appearance.ContactPhone
        };
    }

    private static string LabelFor(MenuItem item, Language language) =>
        item.Translations.FirstOrDefault(t => t.LanguageId == language.Id)?.Label
        ?? item.Translations.FirstOrDefault()?.Label
        ?? "—";

    private async Task<List<MenuItem>> LoadActiveMenuItemsAsync(int menuId, CancellationToken cancellationToken) =>
        await _db.MenuItems.AsNoTracking()
            .Where(i => i.MenuId == menuId && i.IsActive)
            .Include(i => i.Translations)
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Id)
            .ToListAsync(cancellationToken);

    private async Task<Dictionary<int, string>> LoadSlugMapAsync(
        IReadOnlyList<int> pageIds,
        int languageId,
        CancellationToken cancellationToken)
    {
        if (pageIds.Count == 0)
            return new Dictionary<int, string>();

        var rows = await _db.Pages.AsNoTracking()
            .Where(p => pageIds.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                p.Slug,
                LangSlug = p.Translations.Where(t => t.LanguageId == languageId).Select(t => t.Slug).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(
            x => x.Id,
            x =>
            {
                var s = !string.IsNullOrWhiteSpace(x.LangSlug) ? x.LangSlug!.Trim() : x.Slug.Trim();
                return string.IsNullOrEmpty(s) ? x.Id.ToString() : s;
            });
    }

    private PublicNavLinkVm MapItem(
        MenuItem item,
        ILookup<int?, MenuItem> byParent,
        IReadOnlyDictionary<int, string> slugByPageId,
        Language language,
        string langCode)
    {
        var label = item.Translations.FirstOrDefault(t => t.LanguageId == language.Id)?.Label
            ?? item.Translations.FirstOrDefault()?.Label
            ?? "—";

        var href = BuildHref(item, slugByPageId, langCode);

        var children = byParent[item.Id]
            .Select(c => MapItem(c, byParent, slugByPageId, language, langCode))
            .ToList();

        return new PublicNavLinkVm
        {
            Label = label,
            Href = href,
            Children = children
        };
    }

    private static string BuildHref(MenuItem item, IReadOnlyDictionary<int, string> slugByPageId, string langCode)
    {
        var c = Uri.EscapeDataString(langCode.Trim());

        switch (item.LinkType)
        {
            case MenuLinkType.External:
                return string.IsNullOrWhiteSpace(item.ExternalUrl) ? "#" : item.ExternalUrl.Trim();

            case MenuLinkType.Internal:
                if (item.TargetPageId is not { } pid || !slugByPageId.TryGetValue(pid, out var slug))
                    return "#";
                if (string.IsNullOrWhiteSpace(slug))
                    return $"/{c}";
                return $"/{c}/{Uri.EscapeDataString(slug.Trim())}";

            case MenuLinkType.Anchor:
            {
                var frag = string.IsNullOrWhiteSpace(item.Anchor) ? "" : item.Anchor.Trim();
                if (!string.IsNullOrEmpty(frag) && !frag.StartsWith('#'))
                    frag = "#" + frag;

                if (item.TargetPageId is null)
                    return string.IsNullOrEmpty(frag) ? "#" : frag;

                if (!slugByPageId.TryGetValue(item.TargetPageId.Value, out var anchorSlug))
                    return string.IsNullOrEmpty(frag) ? "#" : frag;

                if (string.IsNullOrWhiteSpace(anchorSlug))
                    return string.IsNullOrEmpty(frag) ? $"/{c}" : $"/{c}{frag}";

                return $"/{c}/{Uri.EscapeDataString(anchorSlug.Trim())}{frag}";

            }
            default:
                return "#";
        }
    }
}
