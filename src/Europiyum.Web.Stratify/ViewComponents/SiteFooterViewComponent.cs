using Europiyum.Cms.Application.Configuration;
using Europiyum.Cms.Application.Public.Models;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Europiyum.Web.Stratify.ViewComponents;

public class SiteFooterViewComponent : ViewComponent
{
    private readonly IPublicMenuService _menus;
    private readonly IPublicAppearanceService _appearance;
    private readonly IOptions<CompanySiteOptions> _site;

    public SiteFooterViewComponent(
        IPublicMenuService menus,
        IPublicAppearanceService appearance,
        IOptions<CompanySiteOptions> site)
    {
        _menus = menus;
        _appearance = appearance;
        _site = site;
    }

    public async Task<IViewComponentResult> InvokeAsync(string? title, string? languageCode)
    {
        var code = _site.Value.CompanyCode;
        if (string.IsNullOrWhiteSpace(code))
            return View("Default", new SiteFooterViewModel { SiteTitle = title ?? string.Empty });

        var lang = languageCode;
        if (string.IsNullOrWhiteSpace(lang))
            lang = ViewContext.HttpContext.Request.Query["culture"].FirstOrDefault();

        var langCode = string.IsNullOrWhiteSpace(lang) ? "tr" : lang.Trim();
        var appearance = await _appearance.GetSnapshotAsync(code);
        if (!string.IsNullOrWhiteSpace(appearance.FooterFullHtml))
        {
            return View("Default", new SiteFooterViewModel
            {
                SiteTitle = title ?? "Site",
                LanguageCode = langCode,
                RenderFullHtmlOnly = true,
                FooterFullHtml = appearance.FooterFullHtml
            });
        }

        var vm = await _menus.BuildFooterAsync(code, title ?? "Site", lang);
        return View("Default", vm);
    }
}
