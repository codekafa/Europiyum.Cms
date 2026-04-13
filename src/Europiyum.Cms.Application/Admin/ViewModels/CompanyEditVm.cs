using System.ComponentModel.DataAnnotations;

namespace Europiyum.Cms.Application.Admin.ViewModels;

public class CompanyEditVm
{
    public int Id { get; set; }

    [Required, MaxLength(256)]
    [Display(Name = "Şirket adı")]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(64)]
    [Display(Name = "Kod (deploy)")]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(128)]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = string.Empty;

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; }

    [MaxLength(512)]
    [Display(Name = "Birincil domain")]
    public string? PrimaryDomain { get; set; }

    [Required, MaxLength(64)]
    [Display(Name = "Anasayfa varyant anahtarı")]
    public string HomepageVariantKey { get; set; } = "index";

    [Display(Name = "Varsayılan dil Id")]
    public int DefaultLanguageId { get; set; }
}
