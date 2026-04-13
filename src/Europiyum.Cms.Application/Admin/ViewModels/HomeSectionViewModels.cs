using System.ComponentModel.DataAnnotations;

namespace Europiyum.Cms.Application.Admin.ViewModels;

public class HomeSectionListItemVm
{
    public int Id { get; set; }

    public string SectionKey { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public int? LinkedComponentItemId { get; set; }

    public string? TitlePreview { get; set; }
}

public class HomeSectionCreateVm
{
    public int CompanyId { get; set; }

    [Required]
    [MaxLength(64)]
    [Display(Name = "Bölüm anahtarı")]
    public string SectionKey { get; set; } = string.Empty;

    [Display(Name = "Sıra")]
    public int SortOrder { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Bağlı bileşen (isteğe bağlı)")]
    public int? LinkedComponentItemId { get; set; }
}

public class HomeSectionEditVm
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    [Required, MaxLength(64)]
    public string SectionKey { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public int? LinkedComponentItemId { get; set; }

    public List<HomeSectionTranslationEditVm> Translations { get; set; } = new();
}

public class HomeSectionTranslationEditVm
{
    public int LanguageId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Subtitle { get; set; }

    public string? BodyHtml { get; set; }

    public string? JsonPayload { get; set; }
}
