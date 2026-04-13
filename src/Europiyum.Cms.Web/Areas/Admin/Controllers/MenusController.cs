using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Application.Services;
using Europiyum.Cms.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Europiyum.Cms.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class MenusController : Controller
{
    private readonly MenuItemAdminService _menuItems;
    private readonly PageAdminService _pages;
    private readonly IAdminWorkspace _workspace;

    public MenusController(MenuItemAdminService menuItems, PageAdminService pages, IAdminWorkspace workspace)
    {
        _menuItems = menuItems;
        _pages = pages;
        _workspace = workspace;
    }

    public async Task<IActionResult> Index(MenuKind kind, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
        {
            TempData["Toast"] = "Önce bir şirket seçin.";
            return RedirectToAction("Index", "Dashboard");
        }

        await _menuItems.EnsureMenuAsync(companyId.Value, kind, cancellationToken);
        var vm = await _menuItems.BuildItemsPageAsync(companyId.Value, kind, cancellationToken);
        return View(vm);
    }

    public async Task<IActionResult> Create(MenuKind kind, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        var menuId = await _menuItems.EnsureMenuAsync(companyId.Value, kind, cancellationToken);
        ViewBag.MenuKind = kind;
        await FillMenuFormBagsAsync(companyId.Value, menuId, excludeItemId: null, cancellationToken);

        return View(new MenuItemCreateVm
        {
            MenuId = menuId,
            CompanyId = companyId.Value,
            SortOrder = 0,
            LinkType = MenuLinkType.Internal
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuItemCreateVm vm, MenuKind kind, CancellationToken cancellationToken)
    {
        if (_workspace.SelectedCompanyId != vm.CompanyId)
            return Forbid();

        if (!ModelState.IsValid)
        {
            ViewBag.MenuKind = kind;
            await FillMenuFormBagsAsync(vm.CompanyId, vm.MenuId, excludeItemId: null, cancellationToken);
            return View(vm);
        }

        var result = await _menuItems.TryCreateAsync(vm, cancellationToken);
        if (!result.Ok)
        {
            ViewBag.MenuKind = kind;
            ModelState.AddModelError(string.Empty, result.Error ?? "Oluşturulamadı.");
            await FillMenuFormBagsAsync(vm.CompanyId, vm.MenuId, excludeItemId: null, cancellationToken);
            return View(vm);
        }

        TempData["Toast"] = "Menü öğesi oluşturuldu.";
        return RedirectToAction(nameof(Index), new { kind });
    }

    public async Task<IActionResult> Edit(int id, MenuKind kind, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        var vm = await _menuItems.GetForEditAsync(id, companyId.Value, cancellationToken);
        if (vm is null)
            return NotFound();

        ViewBag.MenuKind = kind;
        await FillMenuFormBagsAsync(companyId.Value, vm.MenuId, excludeItemId: id, cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MenuItemEditVm vm, MenuKind kind, CancellationToken cancellationToken)
    {
        if (_workspace.SelectedCompanyId != vm.CompanyId)
            return Forbid();

        if (!ModelState.IsValid)
        {
            ViewBag.MenuKind = kind;
            await FillMenuFormBagsAsync(vm.CompanyId, vm.MenuId, excludeItemId: vm.Id, cancellationToken);
            return View(vm);
        }

        try
        {
            await _menuItems.SaveAsync(vm, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            ViewBag.MenuKind = kind;
            ModelState.AddModelError(string.Empty, ex.Message);
            await FillMenuFormBagsAsync(vm.CompanyId, vm.MenuId, excludeItemId: vm.Id, cancellationToken);
            return View(vm);
        }

        TempData["Toast"] = "Menü öğesi kaydedildi.";
        return RedirectToAction(nameof(Index), new { kind });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, MenuKind kind, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        var result = await _menuItems.TryDeleteAsync(id, companyId.Value, cancellationToken);
        if (!result.Ok)
            TempData["Error"] = result.Error;
        else
            TempData["Toast"] = "Menü öğesi silindi.";

        return RedirectToAction(nameof(Index), new { kind });
    }

    private async Task FillMenuFormBagsAsync(
        int companyId,
        int menuId,
        int? excludeItemId,
        CancellationToken cancellationToken)
    {
        ViewBag.Pages = await _pages.ListForCompanyAsync(
            companyId,
            new PageListFilterVm { ActiveOnly = true },
            cancellationToken);
        ViewBag.ParentOptions = await _menuItems.ListParentOptionsAsync(menuId, companyId, excludeItemId, cancellationToken);
    }
}
