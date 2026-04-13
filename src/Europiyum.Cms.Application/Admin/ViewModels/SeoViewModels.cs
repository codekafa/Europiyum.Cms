using System.ComponentModel.DataAnnotations;

namespace Europiyum.Cms.Application.Admin.ViewModels;

public class PageSeoEditVm
{
    public int PageId { get; set; }

    public int CompanyId { get; set; }

    public string PageTitle { get; set; } = string.Empty;

    public List<SeoLanguageRowVm> Rows { get; set; } = new();
}

public class SeoLanguageRowVm
{
    public int LanguageId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public int? SeoMetadataId { get; set; }

    [MaxLength(512)]
    [Display(Name = "Meta title")]
    public string? MetaTitle { get; set; }

    [MaxLength(1024)]
    [Display(Name = "Meta description")]
    public string? MetaDescription { get; set; }

    [MaxLength(512)]
    [Display(Name = "Meta keywords")]
    public string? MetaKeywords { get; set; }

    [MaxLength(1024)]
    [Display(Name = "Canonical URL")]
    public string? CanonicalUrl { get; set; }

    [MaxLength(512)]
    [Display(Name = "OG title")]
    public string? OgTitle { get; set; }

    [MaxLength(1024)]
    [Display(Name = "OG description")]
    public string? OgDescription { get; set; }

    [Display(Name = "OG görsel (medya Id)")]
    public int? OgImageMediaId { get; set; }

    [MaxLength(128)]
    [Display(Name = "Robots")]
    public string? Robots { get; set; }
}
