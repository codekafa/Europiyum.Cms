using Europiyum.Cms.Domain.Common;
using Europiyum.Cms.Domain.Enums;

namespace Europiyum.Cms.Domain.Entities;

public class Page : AuditableEntity
{
    public int CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public PageType PageType { get; set; }

    /// <summary>Neutral slug used when no per-language slug exists.</summary>
    public string Slug { get; set; } = string.Empty;

    public string? TemplateKey { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public int? FormDefinitionId { get; set; }

    public FormDefinition? FormDefinition { get; set; }

    public ICollection<PageTranslation> Translations { get; set; } = new List<PageTranslation>();

    public ICollection<PageComponent> PageComponents { get; set; } = new List<PageComponent>();

    public ICollection<SeoMetadata> SeoEntries { get; set; } = new List<SeoMetadata>();
}
