using System.Net;
using System.Net.Mail;
using Europiyum.Cms.Application.Admin;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Europiyum.Cms.Application.Services;

public class SmtpEmailService : ISmtpEmailService
{
    private readonly CmsDbContext _db;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(CmsDbContext db, ILogger<SmtpEmailService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<CmsOpResult> TrySendPlainAsync(
        int companyId,
        IReadOnlyList<string> recipients,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        if (recipients.Count == 0)
            return CmsOpResult.Fail("Alıcı yok.");

        var smtp = await _db.MailSettings.AsNoTracking()
            .FirstOrDefaultAsync(m => m.CompanyId == companyId, cancellationToken);
        if (smtp is null)
            return CmsOpResult.Fail("SMTP ayarları tanımlı değil.");

        if (string.IsNullOrWhiteSpace(smtp.SmtpHost) || string.IsNullOrWhiteSpace(smtp.SenderEmail))
            return CmsOpResult.Fail("SMTP sunucusu veya gönderen e-posta eksik.");

        try
        {
            using var client = new SmtpClient(smtp.SmtpHost.Trim(), smtp.SmtpPort)
            {
                EnableSsl = smtp.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            if (!string.IsNullOrWhiteSpace(smtp.UserName))
                client.Credentials = new NetworkCredential(smtp.UserName.Trim(), smtp.Password ?? "");

            using var message = new MailMessage
            {
                From = new MailAddress(smtp.SenderEmail.Trim(), smtp.SenderName.Trim()),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            foreach (var addr in recipients)
            {
                if (IsPlausibleEmail(addr))
                    message.To.Add(addr.Trim());
            }

            if (message.To.Count == 0)
                return CmsOpResult.Fail("Geçerli alıcı adresi yok.");

            await client.SendMailAsync(message, cancellationToken);
            return CmsOpResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SMTP gönderimi başarısız (company {CompanyId})", companyId);
            return CmsOpResult.Fail("E-posta gönderilemedi.");
        }
    }

    private static bool IsPlausibleEmail(string s)
    {
        s = s.Trim();
        if (s.Length < 5 || s.Length > 256)
            return false;
        return s.Contains('@', StringComparison.Ordinal) && !s.Contains(' ', StringComparison.Ordinal);
    }
}
