using Europiyum.Cms.Application.Admin;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Europiyum.Cms.Application.Services;

public class ComponentAdminService
{
    private readonly CmsDbContext _db;

    public ComponentAdminService(CmsDbContext db) => _db = db;

    public async Task<IReadOnlyList<ComponentTypeOptionVm>> ListComponentTypesAsync(CancellationToken cancellationToken = default)
    {
        var rows = await _db.ComponentTypes.AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.DisplayName)
            .Select(t => new { t.Id, t.Key, t.DisplayName })
            .ToListAsync(cancellationToken);
        return rows
            .Select(t => new ComponentTypeOptionVm
            {
                Id = t.Id,
                Key = t.Key,
                DisplayName = t.DisplayName,
                Description = ComponentTypeHelpTexts.GetDescription(t.Key)
            })
            .ToList();
    }

    public async Task<IReadOnlyList<ComponentListItemVm>> ListForCompanyAsync(
        int companyId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var defaultLang = await _db.Companies.AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => c.DefaultLanguageId)
            .FirstAsync(cancellationToken);

        var q = _db.ComponentItems.AsNoTracking()
            .Where(c => c.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(c =>
                EF.Functions.ILike(c.ComponentType.Key, $"%{s}%")
                || EF.Functions.ILike(c.ComponentType.DisplayName, $"%{s}%")
                || c.Translations.Any(t => t.Title != null && EF.Functions.ILike(t.Title, $"%{s}%")));
        }

        return await q
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .Select(c => new ComponentListItemVm
            {
                Id = c.Id,
                TypeKey = c.ComponentType.Key,
                TypeDisplayName = c.ComponentType.DisplayName,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                TitlePreview = c.Translations.Where(t => t.LanguageId == defaultLang).Select(t => t.Title).FirstOrDefault()
                    ?? c.Translations.Select(t => t.Title).FirstOrDefault(),
                PrimaryMediaId = c.PrimaryMediaId
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ComponentEditVm?> GetForEditAsync(int id, int companyId, CancellationToken cancellationToken = default)
    {
        var item = await _db.ComponentItems
            .Include(c => c.ComponentType)
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId, cancellationToken);
        if (item is null)
            return null;

        var langs = await CompanyLanguageHelper.GetEnabledLanguagesOrderedAsync(_db, companyId, cancellationToken);

        var vm = new ComponentEditVm
        {
            Id = item.Id,
            CompanyId = item.CompanyId,
            TypeKey = item.ComponentType.Key,
            TypeDisplayName = item.ComponentType.DisplayName,
            SortOrder = item.SortOrder,
            IsActive = item.IsActive,
            JsonPayload = item.JsonPayload,
            PrimaryMediaId = item.PrimaryMediaId
        };

        foreach (var lang in langs)
        {
            var tr = item.Translations.FirstOrDefault(t => t.LanguageId == lang.Id);
            vm.Translations.Add(new ComponentTranslationEditVm
            {
                LanguageId = lang.Id,
                LanguageCode = lang.Code,
                Title = tr?.Title,
                Subtitle = tr?.Subtitle,
                Description = tr?.Description,
                BodyHtml = tr?.BodyHtml,
                ButtonText = tr?.ButtonText,
                ButtonUrl = tr?.ButtonUrl,
                ExtraJson = tr?.ExtraJson
            });
        }

        return vm;
    }

    public async Task<CmsOpResult> TryCreateAsync(ComponentCreateVm vm, CancellationToken cancellationToken = default)
    {
        var typeExists = await _db.ComponentTypes.AnyAsync(t => t.Id == vm.ComponentTypeId && t.IsActive, cancellationToken);
        if (!typeExists)
            return CmsOpResult.Fail("Geçersiz bileşen tipi.");

        var langs = await CompanyLanguageHelper.GetEnabledLanguagesOrderedAsync(_db, vm.CompanyId, cancellationToken);
        if (langs.Count == 0)
            return CmsOpResult.Fail("Önce şirket için aktif dil tanımlayın.");

        var item = new ComponentItem
        {
            CompanyId = vm.CompanyId,
            ComponentTypeId = vm.ComponentTypeId,
            SortOrder = vm.SortOrder,
            IsActive = vm.IsActive
        };
        _db.ComponentItems.Add(item);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var lang in langs)
        {
            _db.ComponentTranslations.Add(new ComponentTranslation
            {
                ComponentItemId = item.Id,
                LanguageId = lang.Id,
                Title = "Yeni bileşen"
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return CmsOpResult.Success();
    }

    public async Task SaveAsync(ComponentEditVm vm, CancellationToken cancellationToken = default)
    {
        var item = await _db.ComponentItems
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == vm.Id && c.CompanyId == vm.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException("Bileşen bulunamadı.");

        item.SortOrder = vm.SortOrder;
        item.IsActive = vm.IsActive;
        item.JsonPayload = string.IsNullOrWhiteSpace(vm.JsonPayload) ? null : vm.JsonPayload;
        item.PrimaryMediaId = vm.PrimaryMediaId;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        var allowed = (await CompanyLanguageHelper.GetEnabledLanguagesOrderedAsync(_db, vm.CompanyId, cancellationToken))
            .Select(l => l.Id).ToHashSet();

        foreach (var row in vm.Translations.Where(t => allowed.Contains(t.LanguageId)))
        {
            var tr = item.Translations.FirstOrDefault(t => t.LanguageId == row.LanguageId);
            if (tr is null)
            {
                tr = new ComponentTranslation { ComponentItemId = item.Id, LanguageId = row.LanguageId };
                item.Translations.Add(tr);
                _db.ComponentTranslations.Add(tr);
            }

            tr.Title = string.IsNullOrWhiteSpace(row.Title) ? null : row.Title.Trim();
            tr.Subtitle = string.IsNullOrWhiteSpace(row.Subtitle) ? null : row.Subtitle.Trim();
            tr.Description = string.IsNullOrWhiteSpace(row.Description) ? null : row.Description.Trim();
            tr.BodyHtml = row.BodyHtml;
            tr.ButtonText = string.IsNullOrWhiteSpace(row.ButtonText) ? null : row.ButtonText.Trim();
            tr.ButtonUrl = string.IsNullOrWhiteSpace(row.ButtonUrl) ? null : row.ButtonUrl.Trim();
            tr.ExtraJson = string.IsNullOrWhiteSpace(row.ExtraJson) ? null : row.ExtraJson.Trim();
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<CmsOpResult> TryDeleteAsync(int id, int companyId, CancellationToken cancellationToken = default)
    {
        var item = await _db.ComponentItems.FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId, cancellationToken);
        if (item is null)
            return CmsOpResult.Fail("Bileşen bulunamadı.");

        _db.ComponentItems.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);
        return CmsOpResult.Success();
    }
}
