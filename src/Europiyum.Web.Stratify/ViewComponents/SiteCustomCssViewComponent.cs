using Europiyum.Cms.Application.Configuration;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Europiyum.Web.Stratify.ViewComponents;

/// <summary>Tema CSS dosyalarından sonra eklenen özel kurallar.</summary>
public class SiteCustomCssViewComponent : ViewComponent
{
    private readonly IPublicAppearanceService _appearance;
    private readonly IOptions<CompanySiteOptions> _site;

    public SiteCustomCssViewComponent(IPublicAppearanceService appearance, IOptions<CompanySiteOptions> site)
    {
        _appearance = appearance;
        _site = site;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var code = _site.Value.CompanyCode;
        if (string.IsNullOrWhiteSpace(code))
            return Content(string.Empty);

        var snap = await _appearance.GetSnapshotAsync(code);
        return string.IsNullOrWhiteSpace(snap.CustomCss)
            ? Content(string.Empty)
            : View("Default", snap.CustomCss);
    }
}
