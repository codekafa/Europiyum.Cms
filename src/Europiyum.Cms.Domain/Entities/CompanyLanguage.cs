namespace Europiyum.Cms.Domain.Entities;

public class CompanyLanguage
{
    public int CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public int LanguageId { get; set; }

    public Language Language { get; set; } = null!;

    public bool IsEnabled { get; set; } = true;

    public bool IsDefault { get; set; }

    public int DisplayOrder { get; set; }
}
