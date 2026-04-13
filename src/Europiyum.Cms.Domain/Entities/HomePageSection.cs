using Europiyum.Cms.Domain.Common;

namespace Europiyum.Cms.Domain.Entities;

public class HomePageSection : AuditableEntity
{
    public int CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    /// <summary>hero, about, featured-services, highlights, statistics, cta, references, blog-preview, contact.</summary>
    public string SectionKey { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public int? LinkedComponentItemId { get; set; }

    public ComponentItem? LinkedComponentItem { get; set; }

    public ICollection<HomePageSectionTranslation> Translations { get; set; } = new List<HomePageSectionTranslation>();
}
