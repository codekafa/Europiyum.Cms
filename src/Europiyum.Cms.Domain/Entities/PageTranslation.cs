namespace Europiyum.Cms.Domain.Entities;

public class PageTranslation
{
    public int Id { get; set; }

    public int PageId { get; set; }

    public Page Page { get; set; } = null!;

    public int LanguageId { get; set; }

    public Language Language { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string? HtmlContent { get; set; }

    /// <summary>İç sayfa breadcrumb başlığı; boşsa sayfa başlığı kullanılır.</summary>
    public string? BreadcrumbHeading { get; set; }

    /// <summary>
    /// Breadcrumb arka plan görseli, stratify köküne göre (örn. <c>images/banner/banner-inner.jpg</c>).
    /// </summary>
    public string? BreadcrumbBackgroundPath { get; set; }
}
