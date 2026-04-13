using Europiyum.Cms.Domain.Common;

namespace Europiyum.Cms.Domain.Entities;

public class Language : AuditableEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsRtl { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<CompanyLanguage> CompanyLanguages { get; set; } = new List<CompanyLanguage>();

    public ICollection<PageTranslation> PageTranslations { get; set; } = new List<PageTranslation>();

    public ICollection<ComponentTranslation> ComponentTranslations { get; set; } = new List<ComponentTranslation>();

    public ICollection<HomePageSectionTranslation> HomePageSectionTranslations { get; set; } = new List<HomePageSectionTranslation>();

    public ICollection<MenuItemTranslation> MenuItemTranslations { get; set; } = new List<MenuItemTranslation>();
}
