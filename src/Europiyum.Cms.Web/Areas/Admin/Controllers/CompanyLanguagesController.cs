using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Europiyum.Cms.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class CompanyLanguagesController : Controller
{
    private readonly CompanyLanguageAdminService _service;
    private readonly IAdminWorkspace _workspace;

    public CompanyLanguagesController(CompanyLanguageAdminService service, IAdminWorkspace workspace)
    {
        _service = service;
        _workspace = workspace;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
        {
            TempData["Toast"] = "Önce üst menüden bir şirket seçin.";
            return RedirectToAction("Index", "Dashboard");
        }

        var vm = await _service.GetPageAsync(companyId.Value, cancellationToken);
        if (vm is null)
            return NotFound();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(CompanyLanguagesUpdateVm vm, CancellationToken cancellationToken)
    {
        if (_workspace.SelectedCompanyId != vm.CompanyId)
            return Forbid();

        if (!ModelState.IsValid)
        {
            var page = await _service.GetPageAsync(vm.CompanyId, cancellationToken);
            return page is null ? NotFound() : View("Index", page);
        }

        try
        {
            await _service.ApplyBulkUpdateAsync(vm, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var page = await _service.GetPageAsync(vm.CompanyId, cancellationToken);
            return page is null ? NotFound() : View("Index", page);
        }

        TempData["Toast"] = "Site dilleri güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int companyId, int languageId, CancellationToken cancellationToken)
    {
        if (_workspace.SelectedCompanyId != companyId)
            return Forbid();

        try
        {
            await _service.AddLanguageAsync(companyId, languageId, cancellationToken);
            TempData["Toast"] = "Dil eklendi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int companyId, int languageId, CancellationToken cancellationToken)
    {
        if (_workspace.SelectedCompanyId != companyId)
            return Forbid();

        try
        {
            await _service.RemoveLanguageAsync(companyId, languageId, cancellationToken);
            TempData["Toast"] = "Dil kaldırıldı.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
