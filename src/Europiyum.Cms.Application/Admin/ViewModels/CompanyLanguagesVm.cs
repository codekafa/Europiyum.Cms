using System.ComponentModel.DataAnnotations;

namespace Europiyum.Cms.Application.Admin.ViewModels;

public class CompanyLanguagesPageVm
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public List<CompanyLanguageRowVm> Rows { get; set; } = new();

    public List<LanguageOptionVm> AvailableLanguages { get; set; } = new();
}

public class CompanyLanguageRowVm
{
    public int LanguageId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public bool IsDefault { get; set; }

    public int DisplayOrder { get; set; }
}

public class LanguageOptionVm
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}

public class CompanyLanguagesUpdateVm
{
    [Required]
    public int CompanyId { get; set; }

    public List<CompanyLanguageRowInputVm> Rows { get; set; } = new();

    /// <summary>Varsayılan dil (şirket için tek seçim).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Varsayılan dil seçin")]
    public int DefaultLanguageId { get; set; }
}

public class CompanyLanguageRowInputVm
{
    public int LanguageId { get; set; }

    public bool IsEnabled { get; set; }

    public int DisplayOrder { get; set; }
}
