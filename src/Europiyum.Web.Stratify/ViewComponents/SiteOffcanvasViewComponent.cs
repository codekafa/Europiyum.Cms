using Europiyum.Cms.Application.Configuration;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Europiyum.Web.Stratify.ViewComponents;

public class SiteOffcanvasViewComponent : ViewComponent
{
    private readonly IPublicAppearanceService _appearance;
    private readonly IOptions<CompanySiteOptions> _site;

    public SiteOffcanvasViewComponent(IPublicAppearanceService appearance, IOptions<CompanySiteOptions> site)
    {
        _appearance = appearance;
        _site = site;
    }

    public async Task<IViewComponentResult> InvokeAsync(string? title, string? languageCode)
    {
        var siteTitle = title ?? ViewData["SiteTitle"] as string ?? ViewData["Title"] as string ?? "Site";
        var lang = languageCode ?? ViewData["LanguageCode"] as string;
        var homeHref = string.IsNullOrWhiteSpace(lang)
            ? "/"
            : "/" + Uri.EscapeDataString(lang.Trim());

        var code = _site.Value.CompanyCode;
        var logoHref = "/_content/Europiyum.Web.Stratify/stratify/images/logo/logo-light.png";
        string? belowMenu = null;
        if (!string.IsNullOrWhiteSpace(code))
        {
            var snap = await _appearance.GetSnapshotAsync(code);
            logoHref = snap.OffcanvasLogoHref;
            belowMenu = snap.OffcanvasBelowMenuHtml;
        }

        return View("Default", new SiteOffcanvasVm
        {
            SiteTitle = siteTitle,
            HomeHref = homeHref,
            LogoSrc = logoHref,
            BelowMenuHtml = belowMenu
        });
    }

    public sealed class SiteOffcanvasVm
    {
        public string SiteTitle { get; set; } = string.Empty;

        public string HomeHref { get; set; } = "/";

        public string LogoSrc { get; set; } = string.Empty;

        public string? BelowMenuHtml { get; set; }
    }
}
