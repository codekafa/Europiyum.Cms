namespace Europiyum.Cms.Application.Configuration;

/// <summary>
/// Bound per public web host to isolate data to one company (deploy profile / appsettings).
/// </summary>
public class CompanySiteOptions
{
    public const string SectionName = "CompanySite";

    public string CompanyCode { get; set; } = string.Empty;

    /// <summary>none | compact | full — üst ince şerit; none önerilir üst şerit yokken (menü boşluğu).</summary>
    public string HeaderTopBar { get; set; } = "none";

    public string? HeaderAddress { get; set; }

    public string? HeaderEmail { get; set; }

    /// <summary>Breadcrumb ve iç sayfa şeridinde ana sayfa link metni.</summary>
    public string BreadcrumbHomeLabel { get; set; } = "Anasayfa";
}
