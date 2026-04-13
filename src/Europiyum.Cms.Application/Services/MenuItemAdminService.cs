using Europiyum.Cms.Application.Admin;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Domain.Enums;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Europiyum.Cms.Application.Services;

public class MenuItemAdminService
{
    private readonly CmsDbContext _db;

    public MenuItemAdminService(CmsDbContext db) => _db = db;

    public async Task<int> EnsureMenuAsync(int companyId, MenuKind kind, CancellationToken cancellationToken = default)
    {
        var existing = await _db.Menus
            .Where(m => m.CompanyId == companyId && m.Kind == kind)
            .OrderBy(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing is not null)
            return existing.Id;

        var menu = new Menu { CompanyId = companyId, Kind = kind, Name = "Main" };
        _db.Menus.Add(menu);
        await _db.SaveChangesAsync(cancellationToken);
        return menu.Id;
    }

    public async Task<bool> MenuBelongsToCompanyAsync(int menuId, int companyId, CancellationToken cancellationToken = default) =>
        await _db.Menus.AnyAsync(m => m.Id == menuId && m.CompanyId == companyId, cancellationToken);

    public async Task<MenuItemsPageVm?> BuildItemsPageAsync(int companyId, MenuKind kind, CancellationToken cancellationToken = default)
    {
        var menuId = await EnsureMenuAsync(companyId, kind, cancellationToken);
        var menu = await _db.Menus.AsNoTracking().FirstAsync(m => m.Id == menuId, cancellationToken);

        var defaultLang = await _db.Companies.AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => c.DefaultLanguageId)
            .FirstAsync(cancellationToken);

        var items = await _db.MenuItems.AsNoTracking()
            .Where(i => i.MenuId == menuId)
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Id)
            .Include(i => i.Translations)
            .ToListAsync(cancellationToken);

        var parentLabels = items.ToDictionary(
            i => i.Id,
            i => i.Translations.FirstOrDefault(t => t.LanguageId == defaultLang)?.Label
                ?? i.Translations.FirstOrDefault()?.Label ?? $"#{i.Id}");

        var rows = items.Select(i => new MenuItemListRowVm
        {
            Id = i.Id,
            ParentMenuItemId = i.ParentMenuItemId,
            ParentLabel = i.ParentMenuItemId is { } pid && parentLabels.TryGetValue(pid, out var pl) ? pl : null,
            SortOrder = i.SortOrder,
            LinkType = i.LinkType,
            LinkSummary = SummarizeLink(i),
            LabelPreview = i.Translations.FirstOrDefault(t => t.LanguageId == defaultLang)?.Label
                ?? i.Translations.FirstOrDefault()?.Label ?? "—",
            IsActive = i.IsActive
        }).ToList();

        return new MenuItemsPageVm
        {
            MenuId = menuId,
            Kind = kind,
            KindLabel = MenuKindDisplay(kind),
            Items = rows
        };
    }

    private static string SummarizeLink(MenuItem i) =>
        i.LinkType switch
        {
            MenuLinkType.External => i.ExternalUrl ?? "—",
            MenuLinkType.Internal => i.TargetPageId?.ToString() ?? "—",
            MenuLinkType.Anchor => string.IsNullOrEmpty(i.Anchor) ? "—" : "#" + i.Anchor.TrimStart('#'),
            _ => "—"
        };

    public static string MenuKindDisplay(MenuKind k) => k switch
    {
        MenuKind.Header => "Üst menü (header)",
        MenuKind.Footer => "Alt menü (footer)",
        MenuKind.FooterColumn => "Footer sütunu",
        MenuKind.QuickLinks => "Hızlı linkler",
        MenuKind.Mobile => "Mobil menü",
        _ => k.ToString()
    };

    public async Task<MenuItemEditVm?> GetForEditAsync(int itemId, int companyId, CancellationToken cancellationToken = default)
    {
        var item = await _db.MenuItems
            .Include(i => i.Menu)
            .Include(i => i.Translations)
            .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
        if (item is null || item.Menu.CompanyId != companyId)
            return null;

        var langs = await CompanyLanguageHelper.GetEnabledLanguagesOrderedAsync(_db, companyId, cancellationToken);

        var vm = new MenuItemEditVm
        {
            Id = item.Id,
            MenuId = item.MenuId,
            CompanyId = companyId,
            ParentMenuItemId = item.ParentMenuItemId,
            SortOrder = item.SortOrder,
            LinkType = item.LinkType,
            ExternalUrl = item.ExternalUrl,
            Anchor = item.Anchor,
            TargetPageId = item.TargetPageId,
            IsActive = item.IsActive
        };

        foreach (var lang in langs)
        {
            var tr = item.Translations.FirstOrDefault(t => t.LanguageId == lang.Id);
            vm.Translations.Add(new MenuItemTranslationEditVm
            {
                LanguageId = lang.Id,
                LanguageCode = lang.Code,
                Label = tr?.Label ?? string.Empty
            });
        }

        return vm;
    }

    public async Task<CmsOpResult> TryCreateAsync(MenuItemCreateVm vm, CancellationToken cancellationToken = default)
    {
        if (!await MenuBelongsToCompanyAsync(vm.MenuId, vm.CompanyId, cancellationToken))
            return CmsOpResult.Fail("Menü bulunamadı.");

        if (vm.ParentMenuItemId is not null)
        {
            var ok = await _db.MenuItems.AnyAsync(
                i => i.Id == vm.ParentMenuItemId && i.MenuId == vm.MenuId, cancellationToken);
            if (!ok)
                return CmsOpResult.Fail("Geçersiz üst öğe.");
        }

        if (vm.LinkType == MenuLinkType.Internal && vm.TargetPageId is null)
            return CmsOpResult.Fail("İç link için hedef sayfa seçin.");
        if (vm.LinkType == MenuLinkType.External && string.IsNullOrWhiteSpace(vm.ExternalUrl))
            return CmsOpResult.Fail("Harici URL girin.");
        if (vm.LinkType == MenuLinkType.Anchor && string.IsNullOrWhiteSpace(vm.Anchor))
            return CmsOpResult.Fail("Çapa değeri girin.");

        var langs = await CompanyLanguageHelper.GetEnabledLanguagesOrderedAsync(_db, vm.CompanyId, cancellationToken);
        if (langs.Count == 0)
            return CmsOpResult.Fail("Aktif dil yok.");

        var item = new MenuItem
        {
            MenuId = vm.MenuId,
            ParentMenuItemId = vm.ParentMenuItemId,
            SortOrder = vm.SortOrder,
            LinkType = vm.LinkType,
            ExternalUrl = vm.LinkType == MenuLinkType.External
                ? (string.IsNullOrWhiteSpace(vm.ExternalUrl) ? null : vm.ExternalUrl.Trim())
                : null,
            Anchor = vm.LinkType == MenuLinkType.Anchor
                ? (string.IsNullOrWhiteSpace(vm.Anchor) ? null : vm.Anchor.Trim())
                : null,
            TargetPageId = vm.LinkType is MenuLinkType.Internal or MenuLinkType.Anchor ? vm.TargetPageId : null,
            IsActive = vm.IsActive
        };
        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var lang in langs)
        {
            _db.MenuItemTranslations.Add(new MenuItemTranslation
            {
                MenuItemId = item.Id,
                LanguageId = lang.Id,
                Label = "Yeni menü öğesi"
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        return CmsOpResult.Success();
    }

    public async Task SaveAsync(MenuItemEditVm vm, CancellationToken cancellationToken = default)
    {
        var item = await _db.MenuItems
            .Include(i => i.Menu)
            .Include(i => i.Translations)
            .FirstOrDefaultAsync(i => i.Id == vm.Id, cancellationToken)
            ?? throw new InvalidOperationException("Öğe bulunamadı.");

        if (item.Menu.CompanyId != vm.CompanyId)
            throw new InvalidOperationException("Şirket uyuşmuyor.");

        if (vm.ParentMenuItemId == vm.Id)
            throw new InvalidOperationException("Öğe kendi üstü olamaz.");

        if (vm.ParentMenuItemId is not null)
        {
            var ok = await _db.MenuItems.AnyAsync(
                i => i.Id == vm.ParentMenuItemId && i.MenuId == item.MenuId, cancellationToken);
            if (!ok)
                throw new InvalidOperationException("Geçersiz üst öğe.");
        }

        if (vm.LinkType == MenuLinkType.Internal && vm.TargetPageId is null)
            throw new InvalidOperationException("İç link için hedef sayfa seçin.");
        if (vm.LinkType == MenuLinkType.External && string.IsNullOrWhiteSpace(vm.ExternalUrl))
            throw new InvalidOperationException("Harici URL girin.");
        if (vm.LinkType == MenuLinkType.Anchor && string.IsNullOrWhiteSpace(vm.Anchor))
            throw new InvalidOperationException("Çapa değeri girin.");

        item.ParentMenuItemId = vm.ParentMenuItemId;
        item.SortOrder = vm.SortOrder;
        item.LinkType = vm.LinkType;
        item.ExternalUrl = vm.LinkType == MenuLinkType.External
            ? (string.IsNullOrWhiteSpace(vm.ExternalUrl) ? null : vm.ExternalUrl.Trim())
            : null;
        item.Anchor = vm.LinkType == MenuLinkType.Anchor
            ? (string.IsNullOrWhiteSpace(vm.Anchor) ? null : vm.Anchor.Trim())
            : null;
        item.TargetPageId = vm.LinkType is MenuLinkType.Internal or MenuLinkType.Anchor ? vm.TargetPageId : null;
        item.IsActive = vm.IsActive;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        var allowed = (await CompanyLanguageHelper.GetEnabledLanguagesOrderedAsync(_db, vm.CompanyId, cancellationToken))
            .Select(l => l.Id).ToHashSet();

        foreach (var row in vm.Translations.Where(t => allowed.Contains(t.LanguageId)))
        {
            var tr = item.Translations.FirstOrDefault(t => t.LanguageId == row.LanguageId);
            if (tr is null)
            {
                tr = new MenuItemTranslation { MenuItemId = item.Id, LanguageId = row.LanguageId };
                item.Translations.Add(tr);
                _db.MenuItemTranslations.Add(tr);
            }

            tr.Label = row.Label.Trim();
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<CmsOpResult> TryDeleteAsync(int itemId, int companyId, CancellationToken cancellationToken = default)
    {
        var item = await _db.MenuItems
            .Include(i => i.Menu)
            .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
        if (item is null || item.Menu.CompanyId != companyId)
            return CmsOpResult.Fail("Öğe bulunamadı.");

        var childCount = await _db.MenuItems.CountAsync(i => i.ParentMenuItemId == itemId, cancellationToken);
        if (childCount > 0)
            return CmsOpResult.Fail("Önce alt menü öğelerini silin.");

        _db.MenuItems.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);
        return CmsOpResult.Success();
    }

    public async Task<IReadOnlyList<(int Id, string Label)>> ListParentOptionsAsync(int menuId, int companyId, int? excludeItemId, CancellationToken cancellationToken = default)
    {
        if (!await MenuBelongsToCompanyAsync(menuId, companyId, cancellationToken))
            return Array.Empty<(int, string)>();

        var defaultLang = await _db.Companies.AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => c.DefaultLanguageId)
            .FirstAsync(cancellationToken);

        var q = _db.MenuItems.AsNoTracking()
            .Where(i => i.MenuId == menuId);
        if (excludeItemId is not null)
            q = q.Where(i => i.Id != excludeItemId.Value);

        var raw = await q
            .OrderBy(i => i.SortOrder)
            .Select(i => new
            {
                i.Id,
                Label = i.Translations.Where(t => t.LanguageId == defaultLang).Select(t => t.Label).FirstOrDefault()
                    ?? i.Translations.Select(t => t.Label).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return raw.Select(x => (x.Id, x.Label ?? "#" + x.Id)).ToList();
    }
}
