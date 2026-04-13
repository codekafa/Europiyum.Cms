namespace Europiyum.Cms.Application.Public.Models;

public class PublicHomeSectionBlock
{
    public string SectionKey { get; set; } = string.Empty;

    public string BodyHtml { get; set; } = string.Empty;
}

public class PublicHomeViewModel
{
    public string CompanyName { get; set; } = string.Empty;

    public string CompanyCode { get; set; } = string.Empty;

    /// <summary>Stratify view name stem: Index, Index3, Index5Dark, etc.</summary>
    public string RazorViewName { get; set; } = "Index";

    public string LanguageCode { get; set; } = "tr";

    public string? HeroTitle { get; set; }

    public string? HeroSubtitle { get; set; }

    /// <summary>
    /// Doluysa Stratify varsayılan banner yerine bu HTML (ör. özel slider) gösterilir.
    /// </summary>
    public string? HeroBodyHtml { get; set; }

    /// <summary>
    /// Aktif ana sayfa bölümleri (hero hariç), <see cref="HomePageSection.SortOrder"/> sırasıyla.
    /// İçerik admindeki çeviri <c>BodyHtml</c> alanından gelir.
    /// </summary>
    public IReadOnlyList<PublicHomeSectionBlock> Sections { get; set; } = Array.Empty<PublicHomeSectionBlock>();

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public string? MetaKeywords { get; set; }

    public string? CanonicalUrl { get; set; }

    public string? Robots { get; set; }
}
