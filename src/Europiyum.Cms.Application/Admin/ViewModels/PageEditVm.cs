using System.ComponentModel.DataAnnotations;
using Europiyum.Cms.Domain.Enums;

namespace Europiyum.Cms.Application.Admin.ViewModels;

public class PageEditVm
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    public PageType PageType { get; set; }

    [Required, MaxLength(256)]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(128)]
    [Display(Name = "Şablon anahtarı")]
    public string? TemplateKey { get; set; }

    [Display(Name = "Sıra")]
    public int SortOrder { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; }

    [Display(Name = "Sayfa formu")]
    public int? FormDefinitionId { get; set; }

    public List<PageTranslationEditVm> Translations { get; set; } = new();
}

public class PageTranslationEditVm
{
    public int LanguageId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    [Required, MaxLength(512)]
    [Display(Name = "Başlık")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(256)]
    [Display(Name = "Dil bazlı slug")]
    public string? Slug { get; set; }

    [Display(Name = "HTML içerik")]
    public string? HtmlContent { get; set; }

    [MaxLength(512)]
    [Display(Name = "Breadcrumb başlığı")]
    public string? BreadcrumbHeading { get; set; }

    [MaxLength(512)]
    [Display(Name = "Breadcrumb arka plan görseli")]
    public string? BreadcrumbBackgroundPath { get; set; }
}
