namespace Europiyum.Cms.Application.Public.Models;

public class FooterLinkVm
{
    public string Label { get; set; } = string.Empty;

    public string Href { get; set; } = string.Empty;
}

public class FooterColumnVm
{
    public string Title { get; set; } = string.Empty;

    public List<FooterLinkVm> Links { get; set; } = new();
}

public class SiteFooterViewModel
{
    public string SiteTitle { get; set; } = string.Empty;

    public string LanguageCode { get; set; } = "tr";

    public string FooterLogoSrc { get; set; } = "";

    /// <summary>Doluysa yalnızca <see cref="FooterFullHtml"/> basılır.</summary>
    public bool RenderFullHtmlOnly { get; set; }

    public string? FooterFullHtml { get; set; }

    public string? IntroHtml { get; set; }

    public string? BodyHtml { get; set; }

    /// <summary>Boşsa görünümde yıl + site adı üretilir.</summary>
    public string? CopyrightHtml { get; set; }

    public List<FooterColumnVm> Columns { get; set; } = new();

    public List<FooterLinkVm> BottomLinks { get; set; } = new();

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }
}
