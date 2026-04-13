using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Europiyum.Cms.Web.ViewComponents;

public class AdminTopBarViewComponent : ViewComponent
{
    private readonly CompanyAdminService _companies;
    private readonly IAdminWorkspace _workspace;

    public AdminTopBarViewComponent(CompanyAdminService companies, IAdminWorkspace workspace)
    {
        _companies = companies;
        _workspace = workspace;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var list = await _companies.ListAsync(HttpContext.RequestAborted);
        var vm = new AdminTopBarVm(
            ViewContext.ViewData["Title"]?.ToString() ?? "Panel",
            ViewContext.ViewData["Subtitle"]?.ToString(),
            list,
            _workspace.SelectedCompanyId);
        return View(vm);
    }
}

public record AdminTopBarVm(
    string Title,
    string? Subtitle,
    IReadOnlyList<CompanyListItemVm> Companies,
    int? SelectedCompanyId);
