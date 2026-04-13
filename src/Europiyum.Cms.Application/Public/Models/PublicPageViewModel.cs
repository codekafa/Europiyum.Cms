namespace Europiyum.Cms.Application.Public.Models;

public class PublicPageViewModel
{
    public string CompanyName { get; set; } = string.Empty;

    public string LanguageCode { get; set; } = "tr";

    public string Title { get; set; } = string.Empty;

    public string? HtmlContent { get; set; }

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public string? MetaKeywords { get; set; }

    public string? CanonicalUrl { get; set; }

    public string? Robots { get; set; }

    /// <summary>Breadcrumb H1; boşsa <see cref="Title"/>.</summary>
    public string? BreadcrumbHeading { get; set; }

    /// <summary>Stratify static altında göreli yol, örn. images/banner/banner-inner.jpg</summary>
    public string BreadcrumbBackgroundRelativePath { get; set; } = "images/banner/banner-inner.jpg";

    public PublicPageFormVm? Form { get; set; }
}
