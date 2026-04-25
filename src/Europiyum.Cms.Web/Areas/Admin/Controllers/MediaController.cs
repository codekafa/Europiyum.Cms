using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Europiyum.Cms.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class MediaController : Controller
{
    private readonly MediaAdminService _media;
    private readonly CompanyAdminService _companies;
    private readonly IAdminWorkspace _workspace;

    public MediaController(MediaAdminService media, CompanyAdminService companies, IAdminWorkspace workspace)
    {
        _media = media;
        _companies = companies;
        _workspace = workspace;
    }

    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
        {
            TempData["Toast"] = "Önce bir şirket seçin.";
            return RedirectToAction("Index", "Dashboard");
        }

        ViewBag.Search = search;
        var items = await _media.ListForCompanyAsync(companyId.Value, search, cancellationToken);
        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(List<IFormFile> files, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction(nameof(Index));

        if (files is null || files.Count == 0)
        {
            TempData["Error"] = "En az bir dosya seçin.";
            return RedirectToAction(nameof(Index));
        }

        var company = await _companies.GetForEditAsync(companyId.Value, cancellationToken);
        if (company is null)
        {
            TempData["Error"] = "Şirket bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var uploaded = 0;
        var failures = new List<string>();
        foreach (var file in files.Where(f => f is not null && f.Length > 0))
        {
            await using var stream = file.OpenReadStream();
            var result = await _media.TryUploadAsync(
                companyId.Value,
                company.Code,
                stream,
                file.FileName,
                file.ContentType,
                file.Length,
                cancellationToken);

            if (!result.Ok)
                failures.Add($"{file.FileName}: {result.Error}");
            else
                uploaded++;
        }

        if (uploaded == 0)
            TempData["Error"] = failures.Count > 0 ? string.Join(" | ", failures) : "Yüklenebilir dosya bulunamadı.";
        else if (failures.Count > 0)
            TempData["Toast"] = $"{uploaded} dosya yüklendi. Hata: {string.Join(" | ", failures)}";
        else
            TempData["Toast"] = $"{uploaded} dosya yüklendi.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
            return RedirectToAction(nameof(Index));

        var result = await _media.TryDeleteAsync(id, companyId.Value, cancellationToken);
        if (!result.Ok)
            TempData["Error"] = result.Error;
        else
            TempData["Toast"] = "Medya silindi.";

        return RedirectToAction(nameof(Index));
    }
}
