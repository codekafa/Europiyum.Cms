using System.ComponentModel.DataAnnotations;

namespace Europiyum.Cms.Application.Admin.ViewModels;

public class FormDefinitionCreateVm
{
    public int CompanyId { get; set; }

    [Required(ErrorMessage = "Ad gerekli.")]
    [MaxLength(256)]
    [Display(Name = "Görünen ad")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Anahtar gerekli.")]
    [MaxLength(128)]
    [Display(Name = "Anahtar (URL)")]
    public string Key { get; set; } = string.Empty;

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
