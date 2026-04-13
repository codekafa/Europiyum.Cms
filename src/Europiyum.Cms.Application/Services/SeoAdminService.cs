using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Europiyum.Cms.Application.Services;

public class SeoAdminService
{
    private readonly CmsDbContext _db;

    public SeoAdminService(CmsDbContext db) => _db = db;

    public async Task<PageSeoEditVm?> GetForEditAsync(int pageId, int companyId, CancellationToken cancellationToken = default)
    {
        var page = await _db.Pages
            .AsNoTracking()
            .Include(p => p.SeoEntries)
            .Include(p => p.Translations)
            .FirstOrDefaultAsync(p => p.Id == pageId && p.CompanyId == companyId, cancellationToken);
        if (page is null)
            return null;

        var defaultLang = await _db.Companies.AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => c.DefaultLanguageId)
            .FirstAsync(cancellationToken);

        var pageTitle = page.Translations.FirstOrDefault(t => t.LanguageId == defaultLang)?.Title
            ?? page.Translations.FirstOrDefault()?.Title
            ?? page.Slug;

        var langs = await CompanyLanguageHelper.GetEnabledLanguagesOrderedAsync(_db, companyId, cancellationToken);

        var vm = new PageSeoEditVm
        {
            PageId = page.Id,
            CompanyId = companyId,
            PageTitle = pageTitle
        };

        foreach (var lang in langs)
        {
            var seo = page.SeoEntries.FirstOrDefault(s => s.LanguageId == lang.Id);
            vm.Rows.Add(new SeoLanguageRowVm
            {
                LanguageId = lang.Id,
                LanguageCode = lang.Code,
                SeoMetadataId = seo?.Id,
                MetaTitle = seo?.MetaTitle,
                MetaDescription = seo?.MetaDescription,
                MetaKeywords = seo?.MetaKeywords,
                CanonicalUrl = seo?.CanonicalUrl,
                OgTitle = seo?.OgTitle,
                OgDescription = seo?.OgDescription,
                OgImageMediaId = seo?.OgImageMediaId,
                Robots = seo?.Robots
            });
        }

        return vm;
    }

    public async Task SaveAsync(PageSeoEditVm vm, CancellationToken cancellationToken = default)
    {
        if (!await _db.Pages.AnyAsync(p => p.Id == vm.PageId && p.CompanyId == vm.CompanyId, cancellationToken))
            throw new InvalidOperationException("Sayfa bulunamadı.");

        var allowed = (await CompanyLanguageHelper.GetEnabledLanguagesOrderedAsync(_db, vm.CompanyId, cancellationToken))
            .Select(l => l.Id).ToHashSet();

        foreach (var row in vm.Rows.Where(r => allowed.Contains(r.LanguageId)))
        {
            if (row.SeoMetadataId is { } existingId)
            {
                var entity = await _db.SeoMetadata.FirstOrDefaultAsync(
                    s => s.Id == existingId && s.PageId == vm.PageId,
                    cancellationToken);
                if (entity is null)
                    continue;

                entity.MetaTitle = NullIfWhiteSpace(row.MetaTitle);
                entity.MetaDescription = NullIfWhiteSpace(row.MetaDescription);
                entity.MetaKeywords = NullIfWhiteSpace(row.MetaKeywords);
                entity.CanonicalUrl = NullIfWhiteSpace(row.CanonicalUrl);
                entity.OgTitle = NullIfWhiteSpace(row.OgTitle);
                entity.OgDescription = NullIfWhiteSpace(row.OgDescription);
                entity.OgImageMediaId = row.OgImageMediaId;
                entity.Robots = NullIfWhiteSpace(row.Robots);
            }
            else if (HasAnySeoValue(row))
            {
                _db.SeoMetadata.Add(new SeoMetadata
                {
                    PageId = vm.PageId,
                    LanguageId = row.LanguageId,
                    MetaTitle = NullIfWhiteSpace(row.MetaTitle),
                    MetaDescription = NullIfWhiteSpace(row.MetaDescription),
                    MetaKeywords = NullIfWhiteSpace(row.MetaKeywords),
                    CanonicalUrl = NullIfWhiteSpace(row.CanonicalUrl),
                    OgTitle = NullIfWhiteSpace(row.OgTitle),
                    OgDescription = NullIfWhiteSpace(row.OgDescription),
                    OgImageMediaId = row.OgImageMediaId,
                    Robots = NullIfWhiteSpace(row.Robots)
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string? NullIfWhiteSpace(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static bool HasAnySeoValue(SeoLanguageRowVm row) =>
        !string.IsNullOrWhiteSpace(row.MetaTitle)
        || !string.IsNullOrWhiteSpace(row.MetaDescription)
        || !string.IsNullOrWhiteSpace(row.MetaKeywords)
        || !string.IsNullOrWhiteSpace(row.CanonicalUrl)
        || !string.IsNullOrWhiteSpace(row.OgTitle)
        || !string.IsNullOrWhiteSpace(row.OgDescription)
        || row.OgImageMediaId is not null
        || !string.IsNullOrWhiteSpace(row.Robots);
}
