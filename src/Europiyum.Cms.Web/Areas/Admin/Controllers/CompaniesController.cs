using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Europiyum.Cms.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class CompaniesController : Controller
{
    private readonly CompanyAdminService _service;
    private readonly CompanyLanguageAdminService _companyLanguages;

    public CompaniesController(CompanyAdminService service, CompanyLanguageAdminService companyLanguages)
    {
        _service = service;
        _companyLanguages = companyLanguages;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await _service.ListAsync(cancellationToken);
        return View(items);
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var vm = await _service.GetForEditAsync(id, cancellationToken);
        if (vm is null)
            return NotFound();
        ViewBag.LanguageOptions = await _companyLanguages.GetEnabledLanguageOptionsAsync(id, cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CompanyEditVm vm, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.LanguageOptions = await _companyLanguages.GetEnabledLanguageOptionsAsync(vm.Id, cancellationToken);
            return View(vm);
        }

        try
        {
            await _service.SaveAsync(vm, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.LanguageOptions = await _companyLanguages.GetEnabledLanguageOptionsAsync(vm.Id, cancellationToken);
            return View(vm);
        }

        TempData["Toast"] = "Şirket kaydedildi.";
        return RedirectToAction(nameof(Index));
    }
}
