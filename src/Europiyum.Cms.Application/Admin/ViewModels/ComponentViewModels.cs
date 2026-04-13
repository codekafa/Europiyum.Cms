using System.ComponentModel.DataAnnotations;

namespace Europiyum.Cms.Application.Admin.ViewModels;

public class ComponentListItemVm
{
    public int Id { get; set; }

    public string TypeKey { get; set; } = string.Empty;

    public string TypeDisplayName { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public string? TitlePreview { get; set; }

    public int? PrimaryMediaId { get; set; }
}

public class ComponentCreateVm
{
    public int CompanyId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Bileşen tipi seçin")]
    [Display(Name = "Bileşen tipi")]
    public int ComponentTypeId { get; set; }

    [Display(Name = "Sıra")]
    public int SortOrder { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}

public class ComponentEditVm
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public string TypeKey { get; set; } = string.Empty;

    public string TypeDisplayName { get; set; } = string.Empty;

    [Display(Name = "Sıra")]
    public int SortOrder { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; }

    [Display(Name = "JSON ek alanlar")]
    public string? JsonPayload { get; set; }

    [Display(Name = "Birincil medya")]
    public int? PrimaryMediaId { get; set; }

    public List<ComponentTranslationEditVm> Translations { get; set; } = new();
}

public class ComponentTranslationEditVm
{
    public int LanguageId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Subtitle { get; set; }

    public string? Description { get; set; }

    public string? BodyHtml { get; set; }

    public string? ButtonText { get; set; }

    public string? ButtonUrl { get; set; }

    public string? ExtraJson { get; set; }
}

public class ComponentTypeOptionVm
{
    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Admin oluşturma ekranında gösterilen kısa açıklama.</summary>
    public string Description { get; set; } = string.Empty;
}
