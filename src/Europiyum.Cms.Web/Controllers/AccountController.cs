using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Europiyum.Cms.Application.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Europiyum.Cms.Web.Controllers;

public class AccountController : Controller
{
    private readonly AdminAuthOptions _auth;

    public AccountController(IOptions<AdminAuthOptions> auth) => _auth = auth.Value;

    [HttpGet]
    [AllowAnonymous]
    [Route("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocal(returnUrl);

        return View(new LoginVm { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [Route("login")]
    public async Task<IActionResult> Login([FromForm] LoginVm model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var match = _auth.Users.FirstOrDefault(u =>
            string.Equals(u.Username, model.Username, StringComparison.OrdinalIgnoreCase)
            && u.Password == model.Password);

        if (match is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, match.Username),
            new(ClaimTypes.GivenName, match.DisplayName)
        };
        var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(id),
            new AuthenticationProperties { IsPersistent = model.RememberMe });

        return RedirectToLocal(model.ReturnUrl);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
    }
}

public class LoginVm
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
