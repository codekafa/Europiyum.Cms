using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Europiyum.Cms.Web.Models;

namespace Europiyum.Cms.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() =>
        RedirectToAction("Index", "Dashboard", new { area = "Admin" });

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
