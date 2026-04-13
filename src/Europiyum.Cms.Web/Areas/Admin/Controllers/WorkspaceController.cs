using Europiyum.Cms.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Europiyum.Cms.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class WorkspaceController : Controller
{
    private readonly IAdminWorkspace _workspace;

    public WorkspaceController(IAdminWorkspace workspace) => _workspace = workspace;

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SelectCompany(int companyId)
    {
        _workspace.SetCompany(companyId);
        TempData["Toast"] = "Şirket çalışma alanı güncellendi.";
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ClearCompany()
    {
        _workspace.ClearCompany();
        return RedirectToAction("Index", "Dashboard");
    }
}
