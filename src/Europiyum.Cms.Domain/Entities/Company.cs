using Europiyum.Cms.Domain.Common;

namespace Europiyum.Cms.Domain.Entities;

public class Company : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Stable deploy key: partexo, veraotomotiv, tnmotomotiv, rutenyumsolutions.</summary>
    public string Code { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public string? PrimaryDomain { get; set; }

    public int DefaultLanguageId { get; set; }

    public Language DefaultLanguage { get; set; } = null!;

    /// <summary>Maps to Stratify HTML file stem: index, index-3, index-12-dark, etc.</summary>
    public string HomepageVariantKey { get; set; } = "index";

    public ICollection<CompanyLanguage> CompanyLanguages { get; set; } = new List<CompanyLanguage>();

    public ICollection<Page> Pages { get; set; } = new List<Page>();

    public ICollection<ComponentItem> Components { get; set; } = new List<ComponentItem>();

    public ICollection<HomePageSection> HomePageSections { get; set; } = new List<HomePageSection>();

    public ICollection<Menu> Menus { get; set; } = new List<Menu>();

    public ICollection<MediaFile> MediaFiles { get; set; } = new List<MediaFile>();

    public ICollection<FormDefinition> FormDefinitions { get; set; } = new List<FormDefinition>();

    public MailSetting? MailSetting { get; set; }

    public ICollection<SiteSetting> SiteSettings { get; set; } = new List<SiteSetting>();
}
