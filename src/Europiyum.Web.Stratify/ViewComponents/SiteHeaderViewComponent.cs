using Europiyum.Cms.Application.Configuration;
using Europiyum.Cms.Application.Public.Models;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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
        PopulateLanguageLinks(vm);

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

    private void PopulateLanguageLinks(SiteHeaderViewModel vm)
    {
        if (vm.LanguageOptions.Count == 0)
            return;

        var request = ViewContext.HttpContext.Request;
        var queryDict = request.Query.ToDictionary(kv => kv.Key, kv => (string?)kv.Value.ToString(), StringComparer.OrdinalIgnoreCase);
        queryDict.Remove("culture");

        var path = request.Path.Value ?? "/";
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        var hasCultureSegment = segments.Count > 0 && IsCultureSegment(segments[0]);

        var rewritten = new List<PublicLanguageOptionVm>(vm.LanguageOptions.Count);
        foreach (var lang in vm.LanguageOptions)
        {
            var c = (lang.Code ?? "tr").Trim();
            if (string.IsNullOrWhiteSpace(c))
                c = "tr";

            var seg = new List<string>(segments);
            if (hasCultureSegment)
                seg[0] = c;
            else
                seg.Insert(0, c);

            var localizedPath = "/" + string.Join('/', seg.Select(Uri.EscapeDataString));
            var href = QueryHelpers.AddQueryString(localizedPath, queryDict);
            rewritten.Add(new PublicLanguageOptionVm
            {
                Code = c,
                Label = string.IsNullOrWhiteSpace(lang.Label) ? c.ToUpperInvariant() : lang.Label,
                Href = href,
                IsCurrent = string.Equals(c, vm.LanguageCode, StringComparison.OrdinalIgnoreCase)
            });
        }

        vm.LanguageOptions = rewritten;
    }

    private static bool IsCultureSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var v = value.Trim();
        if (v.Length == 2)
            return char.IsLetter(v[0]) && char.IsLetter(v[1]);
        if (v.Length == 5 && v[2] == '-')
            return char.IsLetter(v[0]) && char.IsLetter(v[1]) && char.IsLetter(v[3]) && char.IsLetter(v[4]);
        return false;
    }
}
