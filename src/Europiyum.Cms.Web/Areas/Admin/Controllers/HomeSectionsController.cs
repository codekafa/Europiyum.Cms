using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Europiyum.Cms.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class HomeSectionsController : Controller
{
    private readonly HomePageSectionAdminService _sections;
    private readonly ComponentAdminService _components;
    private readonly IAdminWorkspace _workspace;

    public HomeSectionsController(
        HomePageSectionAdminService sections,
        ComponentAdminService components,
        IAdminWorkspace workspace)
    {
        _sections = sections;
        _components = components;
        _workspace = workspace;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
        {
            TempData["Toast"] = "Önce bir şirket seçin.";
            return RedirectToAction("Index", "Dashboard");
        }

        var items = await _sections.ListForCompanyAsync(companyId.Value, cancellationToken);
        return View(items);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        ViewBag.ComponentOptions = await _components.ListForCompanyAsync(companyId.Value, null, cancellationToken);
        return View(new HomeSectionCreateVm { CompanyId = companyId.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HomeSectionCreateVm vm, CancellationToken cancellationToken)
    {
        if (_workspace.SelectedCompanyId != vm.CompanyId)
            return Forbid();

        if (!ModelState.IsValid)
        {
            ViewBag.ComponentOptions = await _components.ListForCompanyAsync(vm.CompanyId, null, cancellationToken);
            return View(vm);
        }

        var result = await _sections.TryCreateAsync(vm, cancellationToken);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Oluşturulamadı.");
            ViewBag.ComponentOptions = await _components.ListForCompanyAsync(vm.CompanyId, null, cancellationToken);
            return View(vm);
        }

        TempData["Toast"] = "Anasayfa bölümü eklendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        var vm = await _sections.GetForEditAsync(id, companyId.Value, cancellationToken);
        if (vm is null)
            return NotFound();

        ViewBag.ComponentOptions = await _components.ListForCompanyAsync(companyId.Value, null, cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(HomeSectionEditVm vm, CancellationToken cancellationToken)
    {
        if (_workspace.SelectedCompanyId != vm.CompanyId)
            return Forbid();

        if (!ModelState.IsValid)
        {
            ViewBag.ComponentOptions = await _components.ListForCompanyAsync(vm.CompanyId, null, cancellationToken);
            return View(vm);
        }

        await _sections.SaveAsync(vm, cancellationToken);
        TempData["Toast"] = "Bölüm kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        var result = await _sections.TryDeleteAsync(id, companyId.Value, cancellationToken);
        if (!result.Ok)
            TempData["Error"] = result.Error;
        else
            TempData["Toast"] = "Bölüm silindi.";

        return RedirectToAction(nameof(Index));
    }
}
