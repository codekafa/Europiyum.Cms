using Europiyum.Cms.Application.Configuration;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Europiyum.Web.Stratify.Controllers;

public class FormsController : Controller
{
    private readonly IFormSubmissionService _forms;
    private readonly CompanySiteOptions _site;

    public FormsController(IFormSubmissionService forms, IOptions<CompanySiteOptions> site)
    {
        _forms = forms;
        _site = site.Value;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(string culture, string formKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_site.CompanyCode))
            return BadRequest();

        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var k in Request.Form.Keys)
        {
            if (k.StartsWith("__", StringComparison.Ordinal))
                continue;
            dict[k] = Request.Form[k].ToString();
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _forms.TrySubmitAsync(_site.CompanyCode, formKey, dict, ip, cancellationToken);
        if (!result.Ok)
        {
            TempData["FormError"] = result.ErrorMessage ?? "Gönderilemedi.";
            return RedirectToLocalReferer(culture);
        }

        TempData["FormOk"] = "Mesajınız alındı. Teşekkür ederiz.";
        return RedirectToLocalReferer(culture);
    }

    private IActionResult RedirectToLocalReferer(string culture)
    {
        var referer = Request.Headers.Referer.ToString();
        if (Uri.TryCreate(referer, UriKind.Absolute, out var u)
            && (u.Host == Request.Host.Host || string.Equals(u.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase)))
        {
            return Redirect(referer);
        }

        var c = Uri.EscapeDataString(culture.Trim());
        return Redirect("/" + c);
    }
}
