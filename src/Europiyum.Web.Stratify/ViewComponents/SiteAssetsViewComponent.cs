using Europiyum.Cms.Application.Configuration;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Europiyum.Web.Stratify.ViewComponents;

public class SiteAssetsViewComponent : ViewComponent
{
    private readonly IPublicAppearanceService _appearance;
    private readonly IOptions<CompanySiteOptions> _site;

    public SiteAssetsViewComponent(IPublicAppearanceService appearance, IOptions<CompanySiteOptions> site)
    {
        _appearance = appearance;
        _site = site;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var code = _site.Value.CompanyCode;
        if (string.IsNullOrWhiteSpace(code))
        {
            return View("Default", new SiteAssetsVm
            {
                FaviconHref = "/_content/Europiyum.Web.Stratify/stratify/images/favicon.png"
            });
        }

        var snap = await _appearance.GetSnapshotAsync(code);
        return View("Default", new SiteAssetsVm { FaviconHref = snap.FaviconHref });
    }

    public sealed class SiteAssetsVm
    {
        public string FaviconHref { get; set; } = string.Empty;
    }
}
