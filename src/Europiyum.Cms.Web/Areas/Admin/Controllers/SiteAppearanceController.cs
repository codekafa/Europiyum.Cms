using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Admin;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Europiyum.Cms.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class SiteAppearanceController : Controller
{
    private readonly SiteSettingAdminService _settings;
    private readonly IAdminWorkspace _workspace;
    private readonly MediaAdminService _media;
    private readonly CompanyAdminService _companies;

    public SiteAppearanceController(
        SiteSettingAdminService settings,
        IAdminWorkspace workspace,
        MediaAdminService media,
        CompanyAdminService companies)
    {
        _settings = settings;
        _workspace = workspace;
        _media = media;
        _companies = companies;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
        {
            TempData["Toast"] = "Önce bir şirket seçin.";
            return RedirectToAction("Index", "Dashboard");
        }

        var map = await _settings.GetMapAsync(companyId.Value, cancellationToken);
        string Get(string key) => map.TryGetValue(key, out var v) ? v : string.Empty;

        var vm = new SiteAppearanceEditVm
        {
            CompanyId = companyId.Value,
            FooterIntroHtml = Get(SiteSettingKeys.FooterIntroHtml),
            FooterBodyHtml = Get(SiteSettingKeys.FooterBodyHtml),
            FooterCopyrightHtml = Get(SiteSettingKeys.FooterCopyrightHtml),
            FooterFullHtml = Get(SiteSettingKeys.FooterFullHtml),
            OffcanvasBelowMenuHtml = Get(SiteSettingKeys.OffcanvasBelowMenuHtml),
            BrandingFooterLogoPath = Get(SiteSettingKeys.BrandingFooterLogoPath),
            BrandingHeaderLogoMainPath = Get(SiteSettingKeys.BrandingHeaderLogoMainPath),
            BrandingHeaderLogoLightPath = Get(SiteSettingKeys.BrandingHeaderLogoLightPath),
            BrandingHeaderLogoBlackPath = Get(SiteSettingKeys.BrandingHeaderLogoBlackPath),
            BrandingOffcanvasLogoPath = Get(SiteSettingKeys.BrandingOffcanvasLogoPath),
            BrandingFaviconPath = Get(SiteSettingKeys.BrandingFaviconPath),
            OffcanvasLogoVariant = string.IsNullOrWhiteSpace(Get(SiteSettingKeys.OffcanvasLogoVariant))
                ? "light"
                : Get(SiteSettingKeys.OffcanvasLogoVariant),
            SiteContactEmail = Get(SiteSettingKeys.SiteContactEmail),
            SiteContactPhone = Get(SiteSettingKeys.SiteContactPhone),
            HeadScriptsHtml = Get(SiteSettingKeys.HeadScriptsHtml),
            CustomCss = Get(SiteSettingKeys.CustomCss)
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        SiteAppearanceEditVm vm,
        IFormFile? uploadHeaderMain,
        IFormFile? uploadHeaderLight,
        IFormFile? uploadHeaderBlack,
        IFormFile? uploadOffcanvasLogo,
        IFormFile? uploadFooterLogo,
        IFormFile? uploadFavicon,
        CancellationToken cancellationToken)
    {
        if (_workspace.SelectedCompanyId != vm.CompanyId)
            return Forbid();

        var company = await _companies.GetForEditAsync(vm.CompanyId, cancellationToken);
        if (company is null)
            return NotFound();

        async Task TryApplyUploadAsync(IFormFile? file, Action<string> setStoredPath)
        {
            if (file is null || file.Length == 0)
                return;
            await using var stream = file.OpenReadStream();
            var (res, url) = await _media.TryUploadWithPublicUrlAsync(
                vm.CompanyId,
                company.Code,
                stream,
                file.FileName,
                file.ContentType,
                file.Length,
                cancellationToken);
            if (res.Ok && url is not null)
            {
                var absolute = $"{Request.Scheme}://{Request.Host}{url}";
                setStoredPath(absolute);
            }
        }

        await TryApplyUploadAsync(uploadHeaderMain, v => vm.BrandingHeaderLogoMainPath = v);
        await TryApplyUploadAsync(uploadHeaderLight, v => vm.BrandingHeaderLogoLightPath = v);
        await TryApplyUploadAsync(uploadHeaderBlack, v => vm.BrandingHeaderLogoBlackPath = v);
        await TryApplyUploadAsync(uploadOffcanvasLogo, v => vm.BrandingOffcanvasLogoPath = v);
        await TryApplyUploadAsync(uploadFooterLogo, v => vm.BrandingFooterLogoPath = v);
        await TryApplyUploadAsync(uploadFavicon, v => vm.BrandingFaviconPath = v);

        var dict = new Dictionary<string, string?>
        {
            [SiteSettingKeys.FooterIntroHtml] = vm.FooterIntroHtml,
            [SiteSettingKeys.FooterBodyHtml] = vm.FooterBodyHtml,
            [SiteSettingKeys.FooterCopyrightHtml] = vm.FooterCopyrightHtml,
            [SiteSettingKeys.FooterFullHtml] = vm.FooterFullHtml,
            [SiteSettingKeys.OffcanvasBelowMenuHtml] = vm.OffcanvasBelowMenuHtml,
            [SiteSettingKeys.BrandingFooterLogoPath] = vm.BrandingFooterLogoPath,
            [SiteSettingKeys.BrandingHeaderLogoMainPath] = vm.BrandingHeaderLogoMainPath,
            [SiteSettingKeys.BrandingHeaderLogoLightPath] = vm.BrandingHeaderLogoLightPath,
            [SiteSettingKeys.BrandingHeaderLogoBlackPath] = vm.BrandingHeaderLogoBlackPath,
            [SiteSettingKeys.BrandingOffcanvasLogoPath] = vm.BrandingOffcanvasLogoPath,
            [SiteSettingKeys.BrandingFaviconPath] = vm.BrandingFaviconPath,
            [SiteSettingKeys.OffcanvasLogoVariant] = vm.OffcanvasLogoVariant,
            [SiteSettingKeys.SiteContactEmail] = vm.SiteContactEmail,
            [SiteSettingKeys.SiteContactPhone] = vm.SiteContactPhone,
            [SiteSettingKeys.HeadScriptsHtml] = vm.HeadScriptsHtml,
            [SiteSettingKeys.CustomCss] = vm.CustomCss
        };

        await _settings.UpsertAsync(vm.CompanyId, dict, cancellationToken);
        TempData["Toast"] = "Site görünüm ayarları kaydedildi.";
        return RedirectToAction(nameof(Index));
    }
}
