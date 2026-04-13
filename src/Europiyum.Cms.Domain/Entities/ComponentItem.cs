using Europiyum.Cms.Domain.Common;

namespace Europiyum.Cms.Domain.Entities;

public class ComponentItem : AuditableEntity
{
    public int CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public int ComponentTypeId { get; set; }

    public ComponentType ComponentType { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public string? JsonPayload { get; set; }

    public int? PrimaryMediaId { get; set; }

    public MediaFile? PrimaryMedia { get; set; }

    public ICollection<ComponentTranslation> Translations { get; set; } = new List<ComponentTranslation>();

    public ICollection<PageComponent> PageComponents { get; set; } = new List<PageComponent>();

    public ICollection<HomePageSection> HomePageSections { get; set; } = new List<HomePageSection>();
}
