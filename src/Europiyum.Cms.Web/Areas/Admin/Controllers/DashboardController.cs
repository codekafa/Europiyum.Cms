using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Europiyum.Cms.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class DashboardController : Controller
{
    private readonly CompanyAdminService _companies;
    private readonly IAdminWorkspace _workspace;

    public DashboardController(CompanyAdminService companies, IAdminWorkspace workspace)
    {
        _companies = companies;
        _workspace = workspace;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var list = await _companies.ListAsync(cancellationToken);
        var selectedId = _workspace.SelectedCompanyId;
        var selected = selectedId is null ? null : list.FirstOrDefault(c => c.Id == selectedId);
        return View(new DashboardVm(list, selected));
    }
}

public record DashboardVm(IReadOnlyList<CompanyListItemVm> Companies, CompanyListItemVm? Selected);
