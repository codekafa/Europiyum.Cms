using System.Diagnostics;
using Europiyum.Cms.Application.Configuration;
using Europiyum.Cms.Application.Public;
using Europiyum.Cms.Application.Public.Models;
using Europiyum.Cms.Application.Services;
using Europiyum.Web.Stratify.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Europiyum.Web.Stratify.Controllers;

public class HomeController : Controller
{
    private readonly IPublicContentService _content;
    private readonly CompanySiteOptions _site;
    private readonly IAntiforgery _antiforgery;

    public HomeController(IPublicContentService content, IOptions<CompanySiteOptions> site, IAntiforgery antiforgery)
    {
        _content = content;
        _site = site.Value;
        _antiforgery = antiforgery;
    }

    public async Task<IActionResult> Index(string? culture, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_site.CompanyCode))
            return BadRequest("CompanySite:CompanyCode is not configured.");

        var vm = await _content.GetHomeAsync(_site.CompanyCode, culture, cancellationToken);
        if (vm is null)
            return NotFound();

        ViewData["Title"] = vm.MetaTitle ?? vm.CompanyName;
        ViewData["SiteTitle"] = vm.CompanyName;
        ViewData["LanguageCode"] = vm.LanguageCode;
        ViewData["MetaTitle"] = vm.MetaTitle;
        ViewData["MetaDescription"] = vm.MetaDescription;
        ViewData["MetaKeywords"] = vm.MetaKeywords;
        ViewData["CanonicalUrl"] = vm.CanonicalUrl;
        ViewData["Robots"] = vm.Robots;

        ApplyHomeAntiforgeryTokens(vm);

        return View(vm.RazorViewName, vm);
    }

    private void ApplyHomeAntiforgeryTokens(PublicHomeViewModel vm)
    {
        var heroNeeds = !string.IsNullOrEmpty(vm.HeroBodyHtml)
            && (vm.HeroBodyHtml.Contains(CmsPageHtmlTokens.AntiforgeryRequestToken, StringComparison.Ordinal)
                || vm.HeroBodyHtml.Contains(CmsPageHtmlTokens.LanguageCode, StringComparison.Ordinal));
        var sectionNeeds = vm.Sections.Any(s =>
            !string.IsNullOrEmpty(s.BodyHtml)
            && (s.BodyHtml.Contains(CmsPageHtmlTokens.AntiforgeryRequestToken, StringComparison.Ordinal)
                || s.BodyHtml.Contains(CmsPageHtmlTokens.LanguageCode, StringComparison.Ordinal)));
        if (!heroNeeds && !sectionNeeds)
            return;

        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        var token = tokens.RequestToken ?? string.Empty;
        var lang = (vm.LanguageCode ?? "tr").Trim();

        string Replace(string html) => html
            .Replace(CmsPageHtmlTokens.AntiforgeryRequestToken, token, StringComparison.Ordinal)
            .Replace(CmsPageHtmlTokens.LanguageCode, lang, StringComparison.Ordinal);

        if (heroNeeds)
            vm.HeroBodyHtml = Replace(vm.HeroBodyHtml!);

        if (sectionNeeds)
        {
            foreach (var s in vm.Sections)
            {
                if (string.IsNullOrEmpty(s.BodyHtml))
                    continue;
                s.BodyHtml = Replace(s.BodyHtml);
            }
        }
    }

    /// <summary>SEO URL: /{culture}/{slug} (ör. /tr/hakkimizda).</summary>
    public async Task<IActionResult> Page(string culture, string slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_site.CompanyCode))
            return BadRequest("CompanySite:CompanyCode is not configured.");

        var vm = await _content.GetPageBySlugAsync(_site.CompanyCode, slug, culture, cancellationToken);
        if (vm is null)
            return NotFound();

        ViewData["Title"] = vm.MetaTitle ?? vm.Title;
        ViewData["SiteTitle"] = vm.CompanyName;
        ViewData["LanguageCode"] = vm.LanguageCode;
        ViewData["MetaTitle"] = vm.MetaTitle;
        ViewData["MetaDescription"] = vm.MetaDescription;
        ViewData["MetaKeywords"] = vm.MetaKeywords;
        ViewData["CanonicalUrl"] = vm.CanonicalUrl;
        ViewData["Robots"] = vm.Robots;
        ViewData["StratifyHeaderLayout"] = "inner";
        ViewData["BreadcrumbHomeLabel"] = string.IsNullOrWhiteSpace(_site.BreadcrumbHomeLabel)
            ? "Anasayfa"
            : _site.BreadcrumbHomeLabel.Trim();

        if (!string.IsNullOrEmpty(vm.HtmlContent)
            && (vm.HtmlContent.Contains(CmsPageHtmlTokens.AntiforgeryRequestToken, StringComparison.Ordinal)
                || vm.HtmlContent.Contains(CmsPageHtmlTokens.LanguageCode, StringComparison.Ordinal)))
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            var token = tokens.RequestToken;
            if (!string.IsNullOrEmpty(token))
                vm.HtmlContent = vm.HtmlContent.Replace(CmsPageHtmlTokens.AntiforgeryRequestToken, token, StringComparison.Ordinal);
            vm.HtmlContent = vm.HtmlContent.Replace(
                CmsPageHtmlTokens.LanguageCode,
                vm.LanguageCode.Trim(),
                StringComparison.Ordinal);
        }

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
