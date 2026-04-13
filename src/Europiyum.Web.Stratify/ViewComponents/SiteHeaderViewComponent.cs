using Europiyum.Cms.Application.Configuration;
using Europiyum.Cms.Application.Public.Models;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Europiyum.Web.Stratify.ViewComponents;

public class SiteHeaderViewComponent : ViewComponent
{
    private readonly IPublicMenuService _menus;
    private readonly IPublicAppearanceService _appearance;
    private readonly IOptions<CompanySiteOptions> _site;

    public SiteHeaderViewComponent(
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
        var opts = _site.Value;
        var code = opts.CompanyCode;
        SiteHeaderViewModel vm;
        if (string.IsNullOrWhiteSpace(code))
        {
            vm = new SiteHeaderViewModel
            {
                SiteTitle = title ?? string.Empty,
                LanguageCode = string.IsNullOrWhiteSpace(languageCode) ? "tr" : languageCode.Trim()
            };
        }
        else
        {
            var lang = languageCode;
            if (string.IsNullOrWhiteSpace(lang))
                lang = ViewContext.HttpContext.Request.Query["culture"].FirstOrDefault();

            vm = await _menus.BuildHeaderAsync(code, title ?? "Site", lang);
            var snap = await _appearance.GetSnapshotAsync(code);
            vm.HeaderLogoMainSrc = snap.HeaderLogoMainHref;
            vm.HeaderLogoLightSrc = snap.HeaderLogoLightHref;
            vm.HeaderLogoBlackSrc = snap.HeaderLogoBlackHref;
        }

        ApplyHeaderTopFromOptions(vm, opts);
        vm.HomeNavLabel = string.IsNullOrWhiteSpace(opts.BreadcrumbHomeLabel) ? "Anasayfa" : opts.BreadcrumbHomeLabel.Trim();
        var layoutKey = ViewContext.ViewData["StratifyHeaderLayout"] as string;
        vm.Layout = string.Equals(layoutKey, "inner", StringComparison.OrdinalIgnoreCase)
            ? StratifyHeaderLayoutMode.Inner
            : StratifyHeaderLayoutMode.Home;

        return View("Default", vm);
    }

    private static void ApplyHeaderTopFromOptions(SiteHeaderViewModel vm, CompanySiteOptions opts)
    {
        vm.HeaderTopBar = ParseHeaderTopBar(opts.HeaderTopBar);
        vm.HeaderAddress = opts.HeaderAddress;
        vm.HeaderEmail = opts.HeaderEmail;
    }

    private static StratifyHeaderTopBarMode ParseHeaderTopBar(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "full" => StratifyHeaderTopBarMode.Full,
            "compact" => StratifyHeaderTopBarMode.Compact,
            _ => StratifyHeaderTopBarMode.None
        };
}
