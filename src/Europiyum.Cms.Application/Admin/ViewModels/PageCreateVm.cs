using System.ComponentModel.DataAnnotations;
using Europiyum.Cms.Domain.Enums;

namespace Europiyum.Cms.Application.Admin.ViewModels;

public class PageCreateVm
{
    public int CompanyId { get; set; }

    [Required(ErrorMessage = "Slug gerekli")]
    [MaxLength(256)]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = string.Empty;

    [Display(Name = "Sayfa tipi")]
    public PageType PageType { get; set; } = PageType.Standard;

    [MaxLength(128)]
    [Display(Name = "Şablon anahtarı")]
    public string? TemplateKey { get; set; }

    [Display(Name = "Sıra")]
    public int SortOrder { get; set; }
}
