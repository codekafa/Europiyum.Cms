using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Europiyum.Cms.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class LanguagesController : Controller
{
    private readonly LanguageAdminService _service;

    public LanguagesController(LanguageAdminService service) => _service = service;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var items = await _service.ListAsync(cancellationToken);
        return View(items);
    }
}
