using System.ComponentModel.DataAnnotations;
using Europiyum.Cms.Domain.Enums;

namespace Europiyum.Cms.Application.Admin.ViewModels;

public class MenuItemsPageVm
{
    public int MenuId { get; set; }

    public MenuKind Kind { get; set; }

    public string KindLabel { get; set; } = string.Empty;

    public List<MenuItemListRowVm> Items { get; set; } = new();
}

public class MenuItemListRowVm
{
    public int Id { get; set; }

    public int? ParentMenuItemId { get; set; }

    public string? ParentLabel { get; set; }

    public int SortOrder { get; set; }

    public MenuLinkType LinkType { get; set; }

    public string? LinkSummary { get; set; }

    public string LabelPreview { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

public class MenuItemEditVm
{
    public int Id { get; set; }

    public int MenuId { get; set; }

    public int CompanyId { get; set; }

    public int? ParentMenuItemId { get; set; }

    [Display(Name = "Sıra")]
    public int SortOrder { get; set; }

    [Display(Name = "Bağlantı tipi")]
    public MenuLinkType LinkType { get; set; }

    [MaxLength(2048)]
    [Display(Name = "Harici URL")]
    public string? ExternalUrl { get; set; }

    [MaxLength(256)]
    [Display(Name = "Çapa (#id)")]
    public string? Anchor { get; set; }

    [Display(Name = "Hedef sayfa (iç link)")]
    public int? TargetPageId { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public List<MenuItemTranslationEditVm> Translations { get; set; } = new();
}

public class MenuItemTranslationEditVm
{
    public int LanguageId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    [Display(Name = "Menü etiketi")]
    public string Label { get; set; } = string.Empty;
}

public class MenuItemCreateVm
{
    public int MenuId { get; set; }

    public int CompanyId { get; set; }

    public int? ParentMenuItemId { get; set; }

    public int SortOrder { get; set; }

    public MenuLinkType LinkType { get; set; }

    public string? ExternalUrl { get; set; }

    public string? Anchor { get; set; }

    public int? TargetPageId { get; set; }

    public bool IsActive { get; set; } = true;
}
