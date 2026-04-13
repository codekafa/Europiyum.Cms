using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Europiyum.Cms.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class PageAttachmentsController : Controller
{
    private readonly PageAttachmentAdminService _attachments;
    private readonly ComponentAdminService _components;
    private readonly IAdminWorkspace _workspace;

    public PageAttachmentsController(
        PageAttachmentAdminService attachments,
        ComponentAdminService components,
        IAdminWorkspace workspace)
    {
        _attachments = attachments;
        _components = components;
        _workspace = workspace;
    }

    public async Task<IActionResult> Index(int pageId, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        var vm = await _attachments.GetPageWithAttachmentsAsync(pageId, companyId.Value, cancellationToken);
        if (vm is null)
            return NotFound();

        ViewBag.ComponentOptions = await _components.ListForCompanyAsync(companyId.Value, null, cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Attach([FromForm] PageAttachComponentVm vm, CancellationToken cancellationToken)
    {
        if (_workspace.SelectedCompanyId != vm.CompanyId)
            return Forbid();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Index), new { pageId = vm.PageId });
        }

        var result = await _attachments.TryAttachAsync(vm.PageId, vm.CompanyId, vm.ComponentItemId, vm.SortOrder, cancellationToken);
        if (!result.Ok)
            TempData["Error"] = result.Error;
        else
            TempData["Toast"] = "Bileşen sayfaya bağlandı.";

        return RedirectToAction(nameof(Index), new { pageId = vm.PageId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Detach(int pageComponentId, int pageId, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction("Index", "Dashboard");

        var result = await _attachments.TryDetachAsync(pageComponentId, companyId.Value, cancellationToken);
        if (!result.Ok)
            TempData["Error"] = result.Error;
        else
            TempData["Toast"] = "Bağlantı kaldırıldı.";

        return RedirectToAction(nameof(Index), new { pageId });
    }
}
