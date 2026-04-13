using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Application.Services;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Europiyum.Cms.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class FormDefinitionsController : Controller
{
    private readonly CmsDbContext _db;
    private readonly IAdminWorkspace _workspace;
    private readonly FormDefinitionAdminService _forms;

    public FormDefinitionsController(CmsDbContext db, IAdminWorkspace workspace, FormDefinitionAdminService forms)
    {
        _db = db;
        _workspace = workspace;
        _forms = forms;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
        {
            TempData["Toast"] = "Önce bir şirket seçin.";
            return RedirectToAction("Index", "Dashboard");
        }

        var rows = await _db.FormDefinitions.AsNoTracking()
            .Where(f => f.CompanyId == companyId)
            .OrderBy(f => f.Name)
            .Select(f => new FormDefinitionListRowVm
            {
                Id = f.Id,
                Name = f.Name,
                Key = f.Key,
                IsActive = f.IsActive,
                FieldCount = f.Fields.Count
            })
            .ToListAsync(cancellationToken);

        return View(rows);
    }

    public IActionResult Create()
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        return View(new FormDefinitionCreateVm { CompanyId = companyId.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FormDefinitionCreateVm vm, CancellationToken cancellationToken)
    {
        if (_workspace.SelectedCompanyId != vm.CompanyId)
            return Forbid();

        if (!ModelState.IsValid)
            return View(vm);

        var (result, newId) = await _forms.TryCreateAsync(vm, cancellationToken);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Form oluşturulamadı.");
            return View(vm);
        }

        TempData["Toast"] = "Form oluşturuldu. Alanları ekleyebilirsiniz.";
        return RedirectToAction(nameof(Edit), new { id = newId });
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        var vm = await _forms.GetForEditAsync(id, companyId.Value, cancellationToken);
        if (vm is null)
            return NotFound();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(FormDefinitionEditVm vm, CancellationToken cancellationToken)
    {
        if (_workspace.SelectedCompanyId != vm.CompanyId)
            return Forbid();

        vm.Fields ??= new List<FormFieldEditVm>();

        if (!ModelState.IsValid)
            return View(vm);

        var result = await _forms.TrySaveAsync(vm, cancellationToken);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Kaydedilemedi.");
            return View(vm);
        }

        TempData["Toast"] = "Form kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        var result = await _forms.TryDeleteAsync(id, companyId.Value, cancellationToken);
        TempData["Toast"] = result.Ok ? "Form silindi." : (result.Error ?? "Silinemedi.");
        return RedirectToAction(nameof(Index));
    }
}
