using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Europiyum.Cms.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class PageSeoController : Controller
{
    private readonly SeoAdminService _seo;
    private readonly IAdminWorkspace _workspace;

    public PageSeoController(SeoAdminService seo, IAdminWorkspace workspace)
    {
        _seo = seo;
        _workspace = workspace;
    }

    public async Task<IActionResult> Edit(int pageId, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        var vm = await _seo.GetForEditAsync(pageId, companyId.Value, cancellationToken);
        if (vm is null)
            return NotFound();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PageSeoEditVm vm, CancellationToken cancellationToken)
    {
        if (_workspace.SelectedCompanyId != vm.CompanyId)
            return Forbid();

        if (!ModelState.IsValid)
            return View(vm);

        try
        {
            await _seo.SaveAsync(vm, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }

        TempData["Toast"] = "SEO kaydedildi.";
        return RedirectToAction("Edit", "Pages", new { id = vm.PageId });
    }
}
