namespace Europiyum.Cms.Application.Public.Models;

public class SiteHeaderViewModel
{
    public string SiteTitle { get; set; } = string.Empty;

    /// <summary>Menüde ana sayfa linki metni (appsettings CompanySite:BreadcrumbHomeLabel).</summary>
    public string HomeNavLabel { get; set; } = "Anasayfa";

    /// <summary>Resolved language for query string (e.g. culture=).</summary>
    public string LanguageCode { get; set; } = "tr";

    public IReadOnlyList<PublicNavLinkVm> NavLinks { get; set; } = Array.Empty<PublicNavLinkVm>();

    /// <summary>Üst şerit; <see cref="StratifyHeaderTopBarMode.None"/> iken tema top:50px boşluğu kapatılır.</summary>
    public StratifyHeaderTopBarMode HeaderTopBar { get; set; } = StratifyHeaderTopBarMode.None;

    public string? HeaderAddress { get; set; }

    public string? HeaderEmail { get; set; }

    public StratifyHeaderLayoutMode Layout { get; set; } = StratifyHeaderLayoutMode.Home;

    /// <summary>Boşsa görünüm tema varsayılan dosyalarını kullanır.</summary>
    public string? HeaderLogoMainSrc { get; set; }

    public string? HeaderLogoLightSrc { get; set; }

    public string? HeaderLogoBlackSrc { get; set; }
}

public class PublicNavLinkVm
{
    public string Label { get; set; } = string.Empty;

    public string Href { get; set; } = string.Empty;

    public List<PublicNavLinkVm> Children { get; set; } = new();
}
