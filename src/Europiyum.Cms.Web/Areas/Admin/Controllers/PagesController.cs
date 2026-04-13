using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Europiyum.Cms.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class PagesController : Controller
{
    private readonly PageAdminService _pages;
    private readonly IAdminWorkspace _workspace;

    public PagesController(PageAdminService pages, IAdminWorkspace workspace)
    {
        _pages = pages;
        _workspace = workspace;
    }

    public async Task<IActionResult> Index(PageListFilterVm? filter, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
        {
            TempData["Toast"] = "Önce bir şirket seçin.";
            return RedirectToAction("Index", "Dashboard");
        }

        filter ??= new PageListFilterVm();
        var items = await _pages.ListForCompanyAsync(companyId.Value, filter, cancellationToken);
        ViewBag.Filter = filter;
        return View(items);
    }

    public IActionResult Create()
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        return View(new PageCreateVm { CompanyId = companyId.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PageCreateVm vm, CancellationToken cancellationToken)
    {
        if (_workspace.SelectedCompanyId != vm.CompanyId)
            return Forbid();

        if (!ModelState.IsValid)
            return View(vm);

        var result = await _pages.TryCreateAsync(vm, cancellationToken);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Kayıt oluşturulamadı.");
            return View(vm);
        }

        TempData["Toast"] = "Sayfa oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        var vm = await _pages.GetForEditAsync(id, companyId.Value, cancellationToken);
        if (vm is null)
            return NotFound();
        ViewBag.FormDefinitions = await _pages.ListFormDefinitionOptionsAsync(companyId.Value, cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PageEditVm vm, CancellationToken cancellationToken)
    {
        if (_workspace.SelectedCompanyId != vm.CompanyId)
            return Forbid();

        if (!ModelState.IsValid)
        {
            ViewBag.FormDefinitions = await _pages.ListFormDefinitionOptionsAsync(vm.CompanyId, cancellationToken);
            return View(vm);
        }

        await _pages.SaveAsync(vm, cancellationToken);
        TempData["Toast"] = "Sayfa kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        var result = await _pages.TryDeleteAsync(id, companyId.Value, cancellationToken);
        if (!result.Ok)
            TempData["Error"] = result.Error;
        else
            TempData["Toast"] = "Sayfa silindi.";

        return RedirectToAction(nameof(Index));
    }
}
