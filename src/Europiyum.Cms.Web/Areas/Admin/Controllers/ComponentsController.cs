using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Europiyum.Cms.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class ComponentsController : Controller
{
    private readonly ComponentAdminService _components;
    private readonly MediaAdminService _media;
    private readonly IAdminWorkspace _workspace;

    public ComponentsController(ComponentAdminService components, MediaAdminService media, IAdminWorkspace workspace)
    {
        _components = components;
        _media = media;
        _workspace = workspace;
    }

    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
        {
            TempData["Toast"] = "Önce bir şirket seçin.";
            return RedirectToAction("Index", "Dashboard");
        }

        ViewBag.Search = search;
        var items = await _components.ListForCompanyAsync(companyId.Value, search, cancellationToken);
        return View(items);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        ViewBag.ComponentTypes = await _components.ListComponentTypesAsync(cancellationToken);
        return View(new ComponentCreateVm { CompanyId = companyId.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ComponentCreateVm vm, CancellationToken cancellationToken)
    {
        if (_workspace.SelectedCompanyId != vm.CompanyId)
            return Forbid();

        if (!ModelState.IsValid)
        {
            ViewBag.ComponentTypes = await _components.ListComponentTypesAsync(cancellationToken);
            return View(vm);
        }

        var result = await _components.TryCreateAsync(vm, cancellationToken);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Oluşturulamadı.");
            ViewBag.ComponentTypes = await _components.ListComponentTypesAsync(cancellationToken);
            return View(vm);
        }

        TempData["Toast"] = "Bileşen oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        var vm = await _components.GetForEditAsync(id, companyId.Value, cancellationToken);
        if (vm is null)
            return NotFound();

        ViewBag.MediaPicker = await _media.ListRecentForPickerAsync(companyId.Value, 120, cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ComponentEditVm vm, CancellationToken cancellationToken)
    {
        if (_workspace.SelectedCompanyId != vm.CompanyId)
            return Forbid();

        if (!ModelState.IsValid)
        {
            ViewBag.MediaPicker = await _media.ListRecentForPickerAsync(vm.CompanyId, 120, cancellationToken);
            return View(vm);
        }

        await _components.SaveAsync(vm, cancellationToken);
        TempData["Toast"] = "Bileşen kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        var result = await _components.TryDeleteAsync(id, companyId.Value, cancellationToken);
        if (!result.Ok)
            TempData["Error"] = result.Error;
        else
            TempData["Toast"] = "Bileşen silindi.";

        return RedirectToAction(nameof(Index));
    }
}
