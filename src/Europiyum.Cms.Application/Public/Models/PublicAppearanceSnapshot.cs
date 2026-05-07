namespace Europiyum.Cms.Application.Public.Models;

/// <summary>Site ayarlarından türetilen public görünüm (footer, favicon, offcanvas).</summary>
public class PublicAppearanceSnapshot
{
    public string FaviconHref { get; set; } = "";

    public string FooterLogoHref { get; set; } = "";

    /// <summary>img src — <see cref="HeaderLogoMainHref"/> vb. kullanın.</summary>
    public string HeaderLogoMainHref { get; set; } = "";

    public string HeaderLogoLightHref { get; set; } = "";

    public string HeaderLogoBlackHref { get; set; } = "";

    public string OffcanvasLogoHref { get; set; } = "";

    public string? FooterIntroHtml { get; set; }

    public string? FooterBodyHtml { get; set; }

    public string? FooterCopyrightHtml { get; set; }

    /// <summary>Doluysa footer bileşeni yalnızca bunu basar (dil bağımsız varsayılan).</summary>
    public string? FooterFullHtml { get; set; }

    /// <summary>Dil koduna (lowercase) göre per-language tam footer HTML; dolu değilse <see cref="FooterFullHtml"/> fallback olarak kullanılır.</summary>
    public IReadOnlyDictionary<string, string> FooterFullHtmlByLanguage { get; set; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public string? OffcanvasBelowMenuHtml { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    /// <summary>Head içine yerleştirilecek ham işaretleme (script, noscript, meta).</summary>
    public string? HeadScriptsHtml { get; set; }

    /// <summary>&lt;style&gt; içine sarılacak ham CSS.</summary>
    public string? CustomCss { get; set; }
}
