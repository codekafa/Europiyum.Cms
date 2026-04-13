namespace Europiyum.Cms.Domain.Entities;

public class SeoMetadata
{
    public int Id { get; set; }

    public int PageId { get; set; }

    public Page Page { get; set; } = null!;

    public int LanguageId { get; set; }

    public Language Language { get; set; } = null!;

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public string? MetaKeywords { get; set; }

    public string? CanonicalUrl { get; set; }

    public string? OgTitle { get; set; }

    public string? OgDescription { get; set; }

    public int? OgImageMediaId { get; set; }

    public MediaFile? OgImageMedia { get; set; }

    public string? Robots { get; set; }
}
