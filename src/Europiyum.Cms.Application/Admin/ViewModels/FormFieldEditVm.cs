using System.ComponentModel.DataAnnotations;
using Europiyum.Cms.Domain.Enums;

namespace Europiyum.Cms.Application.Admin.ViewModels;

public class FormFieldEditVm
{
    public int Id { get; set; }

    [MaxLength(128)]
    [Display(Name = "Alan anahtarı")]
    public string FieldKey { get; set; } = string.Empty;

    [Display(Name = "Tür")]
    public FormFieldType FieldType { get; set; } = FormFieldType.Text;

    [Display(Name = "Zorunlu")]
    public bool IsRequired { get; set; }

    [MaxLength(256)]
    [Display(Name = "Etiket")]
    public string? DefaultLabel { get; set; }

    [Display(Name = "Seçenekler (satır başına bir değer; Liste / Radyo)")]
    public string? OptionsText { get; set; }
}
