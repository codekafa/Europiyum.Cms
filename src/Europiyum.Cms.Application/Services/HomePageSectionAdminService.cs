using Europiyum.Cms.Application.Admin;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Europiyum.Cms.Application.Services;

public class HomePageSectionAdminService
{
    private readonly CmsDbContext _db;

    public HomePageSectionAdminService(CmsDbContext db) => _db = db;

    public async Task<IReadOnlyList<HomeSectionListItemVm>> ListForCompanyAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var defaultLang = await _db.Companies.AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => c.DefaultLanguageId)
            .FirstAsync(cancellationToken);

        return await _db.HomePageSections.AsNoTracking()
            .Where(s => s.CompanyId == companyId)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Id)
            .Select(s => new HomeSectionListItemVm
            {
                Id = s.Id,
                SectionKey = s.SectionKey,
                SortOrder = s.SortOrder,
                IsActive = s.IsActive,
                LinkedComponentItemId = s.LinkedComponentItemId,
                TitlePreview = s.Translations.Where(t => t.LanguageId == defaultLang).Select(t => t.Title).FirstOrDefault()
                    ?? s.Translations.Select(t => t.Title).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<HomeSectionEditVm?> GetForEditAsync(int id, int companyId, CancellationToken cancellationToken = default)
    {
        var section = await _db.HomePageSections
            .Include(s => s.Translations)
            .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId, cancellationToken);
        if (section is null)
            return null;

        var langs = await CompanyLanguageHelper.GetEnabledLanguagesOrderedAsync(_db, companyId, cancellationToken);

        var vm = new HomeSectionEditVm
        {
            Id = section.Id,
            CompanyId = section.CompanyId,
            SectionKey = section.SectionKey,
            SortOrder = section.SortOrder,
            IsActive = section.IsActive,
            LinkedComponentItemId = section.LinkedComponentItemId
        };

        foreach (var lang in langs)
        {
            var tr = section.Translations.FirstOrDefault(t => t.LanguageId == lang.Id);
            vm.Translations.Add(new HomeSectionTranslationEditVm
            {
                LanguageId = lang.Id,
                LanguageCode = lang.Code,
                Title = tr?.Title,
                Subtitle = tr?.Subtitle,
                BodyHtml = tr?.BodyHtml,
                JsonPayload = tr?.JsonPayload
            });
        }

        return vm;
    }

    public async Task SaveAsync(HomeSectionEditVm vm, CancellationToken cancellationToken = default)
    {
        var section = await _db.HomePageSections
            .Include(s => s.Translations)
            .FirstOrDefaultAsync(s => s.Id == vm.Id && s.CompanyId == vm.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException("Bölüm bulunamadı.");

        section.SectionKey = vm.SectionKey.Trim();
        section.SortOrder = vm.SortOrder;
        section.IsActive = vm.IsActive;
        section.LinkedComponentItemId = vm.LinkedComponentItemId;
        section.UpdatedAt = DateTimeOffset.UtcNow;

        if (section.LinkedComponentItemId is not null)
        {
            var ok = await _db.ComponentItems.AnyAsync(
                c => c.Id == section.LinkedComponentItemId && c.CompanyId == vm.CompanyId, cancellationToken);
            if (!ok)
                section.LinkedComponentItemId = null;
        }

        var allowed = (await CompanyLanguageHelper.GetEnabledLanguagesOrderedAsync(_db, vm.CompanyId, cancellationToken))
            .Select(l => l.Id).ToHashSet();

        foreach (var row in vm.Translations.Where(t => allowed.Contains(t.LanguageId)))
        {
            var tr = section.Translations.FirstOrDefault(t => t.LanguageId == row.LanguageId);
            if (tr is null)
            {
                tr = new HomePageSectionTranslation { HomePageSectionId = section.Id, LanguageId = row.LanguageId };
                section.Translations.Add(tr);
                _db.HomePageSectionTranslations.Add(tr);
            }

            tr.Title = string.IsNullOrWhiteSpace(row.Title) ? null : row.Title.Trim();
            tr.Subtitle = string.IsNullOrWhiteSpace(row.Subtitle) ? null : row.Subtitle.Trim();
            tr.BodyHtml = row.BodyHtml;
            tr.JsonPayload = string.IsNullOrWhiteSpace(row.JsonPayload) ? null : row.JsonPayload.Trim();
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<CmsOpResult> TryCreateAsync(HomeSectionCreateVm vm, CancellationToken cancellationToken = default)
    {
        var langs = await CompanyLanguageHelper.GetEnabledLanguagesOrderedAsync(_db, vm.CompanyId, cancellationToken);
        if (langs.Count == 0)
            return CmsOpResult.Fail("Önce şirket için aktif dil tanımlayın.");

        if (vm.LinkedComponentItemId is not null)
        {
            var ok = await _db.ComponentItems.AnyAsync(
                c => c.Id == vm.LinkedComponentItemId && c.CompanyId == vm.CompanyId, cancellationToken);
            if (!ok)
                return CmsOpResult.Fail("Bağlı bileşen bu şirkete ait değil.");
        }

        var section = new HomePageSection
        {
            CompanyId = vm.CompanyId,
            SectionKey = vm.SectionKey.Trim(),
            SortOrder = vm.SortOrder,
            IsActive = vm.IsActive,
            LinkedComponentItemId = vm.LinkedComponentItemId
        };
        _db.HomePageSections.Add(section);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var lang in langs)
        {
            _db.HomePageSectionTranslations.Add(new HomePageSectionTranslation
            {
                HomePageSectionId = section.Id,
                LanguageId = lang.Id,
                Title = vm.SectionKey
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return CmsOpResult.Success();
    }

    public async Task<CmsOpResult> TryDeleteAsync(int id, int companyId, CancellationToken cancellationToken = default)
    {
        var section = await _db.HomePageSections.FirstOrDefaultAsync(
            s => s.Id == id && s.CompanyId == companyId, cancellationToken);
        if (section is null)
            return CmsOpResult.Fail("Bölüm bulunamadı.");

        _db.HomePageSections.Remove(section);
        await _db.SaveChangesAsync(cancellationToken);
        return CmsOpResult.Success();
    }
}
