using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Admin.ViewModels;
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

        var row = await _db.MailSettings
            .Include(m => m.LanguageRecipients)
            .FirstOrDefaultAsync(m => m.CompanyId == companyId, cancellationToken);
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
            await _db.Entry(row).Collection(m => m.LanguageRecipients).LoadAsync(cancellationToken);
        }

        var vm = await BuildIndexVmAsync(row, cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(MailSettingsIndexVm vm, CancellationToken cancellationToken)
    {
        var companyId = _workspace.SelectedCompanyId;
        if (companyId is null || companyId != vm.CompanyId)
            return Forbid();

        if (!ModelState.IsValid)
        {
            var rowInvalid = await _db.MailSettings
                .Include(m => m.LanguageRecipients)
                .FirstOrDefaultAsync(m => m.CompanyId == companyId, cancellationToken);
            if (rowInvalid is not null)
                await MergeLanguageRowsFromPostedAsync(vm, rowInvalid, cancellationToken);
            return View(vm);
        }

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

        var existingRecipients = await _db.MailSettingLanguageRecipients
            .Where(r => r.MailSettingId == row.Id)
            .ToListAsync(cancellationToken);
        _db.MailSettingLanguageRecipients.RemoveRange(existingRecipients);

        var allowedLangIds = (await _db.CompanyLanguages.AsNoTracking()
            .Where(cl => cl.CompanyId == companyId && cl.IsEnabled)
            .Select(cl => cl.LanguageId)
            .ToListAsync(cancellationToken)).ToHashSet();

        foreach (var item in vm.LanguageRecipients)
        {
            if (!allowedLangIds.Contains(item.LanguageId))
                continue;
            if (string.IsNullOrWhiteSpace(item.RecipientEmails))
                continue;
            _db.MailSettingLanguageRecipients.Add(new MailSettingLanguageRecipient
            {
                MailSettingId = row.Id,
                LanguageId = item.LanguageId,
                RecipientEmails = item.RecipientEmails.Trim(),
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
        TempData["Toast"] = "E-posta ayarları kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Geçersiz modelde dil satırlarını veritabanıyla doldurur; POST edilen alıcı metinlerini korur.</summary>
    private async Task MergeLanguageRowsFromPostedAsync(
        MailSettingsIndexVm vm,
        MailSetting row,
        CancellationToken cancellationToken)
    {
        var postedByLang = vm.LanguageRecipients.ToDictionary(r => r.LanguageId, r => r.RecipientEmails);
        var fresh = await BuildIndexVmAsync(row, cancellationToken);
        foreach (var line in fresh.LanguageRecipients)
        {
            if (postedByLang.TryGetValue(line.LanguageId, out var emails))
                line.RecipientEmails = emails;
        }

        vm.LanguageRecipients = fresh.LanguageRecipients;
    }

    private async Task<MailSettingsIndexVm> BuildIndexVmAsync(MailSetting row, CancellationToken cancellationToken)
    {
        var byLang = row.LanguageRecipients.ToDictionary(r => r.LanguageId);
        var languages = await _db.CompanyLanguages.AsNoTracking()
            .Where(cl => cl.CompanyId == row.CompanyId && cl.IsEnabled)
            .OrderBy(cl => cl.DisplayOrder)
            .ThenBy(cl => cl.LanguageId)
            .Select(cl => new { cl.LanguageId, cl.Language.Code, cl.Language.Name })
            .ToListAsync(cancellationToken);

        var vm = new MailSettingsIndexVm
        {
            Id = row.Id,
            CompanyId = row.CompanyId,
            SmtpHost = row.SmtpHost,
            SmtpPort = row.SmtpPort,
            UserName = row.UserName,
            Password = null,
            UseSsl = row.UseSsl,
            SenderEmail = row.SenderEmail,
            SenderName = row.SenderName,
            FormRecipientEmails = row.FormRecipientEmails
        };

        foreach (var l in languages)
        {
            byLang.TryGetValue(l.LanguageId, out var rec);
            vm.LanguageRecipients.Add(new MailSettingLanguageRecipientRowVm
            {
                LanguageId = l.LanguageId,
                LanguageCode = l.Code,
                LanguageName = l.Name,
                RecipientEmails = rec?.RecipientEmails
            });
        }

        return vm;
    }
}
