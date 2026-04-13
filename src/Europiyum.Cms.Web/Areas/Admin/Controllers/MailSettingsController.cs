using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Europiyum.Cms.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public class MailSettingsController : Controller
{
    private readonly CmsDbContext _db;
    private readonly IAdminWorkspace _workspace;

    public MailSettingsController(CmsDbContext db, IAdminWorkspace workspace)
    {
        _db = db;
        _workspace = workspace;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null)
        {
            TempData["Toast"] = "Önce bir şirket seçin.";
            return RedirectToAction("Index", "Dashboard");
        }

        var row = await _db.MailSettings.FirstOrDefaultAsync(m => m.CompanyId == companyId, cancellationToken);
        if (row is null)
        {
            row = new MailSetting
            {
                CompanyId = companyId.Value,
                SmtpHost = "smtp.example.com",
                SmtpPort = 587,
                SenderEmail = "noreply@example.com",
                SenderName = "Site",
                UseSsl = true
            };
            _db.MailSettings.Add(row);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return View(row);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(MailSetting vm, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null || companyId != vm.CompanyId)
            return Forbid();

        var row = await _db.MailSettings.FirstOrDefaultAsync(m => m.CompanyId == companyId, cancellationToken);
        if (row is null)
            return NotFound();

        row.SmtpHost = vm.SmtpHost.Trim();
        row.SmtpPort = vm.SmtpPort;
        row.UserName = string.IsNullOrWhiteSpace(vm.UserName) ? null : vm.UserName.Trim();
        if (!string.IsNullOrWhiteSpace(vm.Password))
            row.Password = vm.Password.Trim();
        row.UseSsl = vm.UseSsl;
        row.SenderEmail = vm.SenderEmail.Trim();
        row.SenderName = vm.SenderName.Trim();
        row.FormRecipientEmails = string.IsNullOrWhiteSpace(vm.FormRecipientEmails)
            ? null
            : vm.FormRecipientEmails.Trim();
        row.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        TempData["Toast"] = "E-posta ayarları kaydedildi.";
        return RedirectToAction(nameof(Index));
    }
}
