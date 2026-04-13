using System.Text;
using System.Text.Json;
using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Domain.Enums;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Europiyum.Cms.Application.Services;

public class FormSubmissionService : IFormSubmissionService
{
    private static readonly string[] HoneypotFieldNames =
    {
        "form_botcheck", "botcheck", "email_honeypot", "url_honeypot", "website"
    };

    private readonly CmsDbContext _db;
    private readonly ISmtpEmailService _mail;
    private readonly ILogger<FormSubmissionService> _logger;

    public FormSubmissionService(CmsDbContext db, ISmtpEmailService mail, ILogger<FormSubmissionService> logger)
    {
        _db = db;
        _mail = mail;
        _logger = logger;
    }

    public async Task<FormSubmissionResult> TrySubmitAsync(
        string companyCode,
        string formKey,
        IReadOnlyDictionary<string, string> fields,
        string? submitterIp,
        CancellationToken cancellationToken = default)
    {
        var key = formKey.Trim();
        if (string.IsNullOrEmpty(key))
            return new FormSubmissionResult(false, "Geçersiz form.");

        var companyId = await _db.Companies.AsNoTracking()
            .Where(c => c.Code == companyCode && c.IsActive)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (companyId is null)
            return new FormSubmissionResult(false, "Şirket bulunamadı.");

        foreach (var trap in HoneypotFieldNames)
        {
            if (fields.TryGetValue(trap, out var t) && !string.IsNullOrWhiteSpace(t))
                return new FormSubmissionResult(false, "İşlem tamamlanamadı.");
        }

        var form = await _db.FormDefinitions.AsNoTracking()
            .Include(f => f.Fields)
            .FirstOrDefaultAsync(
                f => f.CompanyId == companyId && f.Key == key && f.IsActive,
                cancellationToken);
        if (form is null)
            return new FormSubmissionResult(false, "Form bulunamadı.");

        var ordered = form.Fields.OrderBy(f => f.SortOrder).ThenBy(f => f.Id).ToList();
        var payload = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in ordered)
        {
            fields.TryGetValue(field.FieldKey, out var raw);
            raw ??= string.Empty;
            var v = raw.Trim();

            if (field.IsRequired && string.IsNullOrEmpty(v))
                return new FormSubmissionResult(false, $"“{field.DefaultLabel ?? field.FieldKey}” zorunludur.");

            if (field.FieldType == FormFieldType.Email && !string.IsNullOrEmpty(v) && !v.Contains('@', StringComparison.Ordinal))
                return new FormSubmissionResult(false, "Geçerli bir e-posta girin.");

            if (field.FieldType is FormFieldType.Select or FormFieldType.Radio)
            {
                if (TryParseOptions(field.OptionsJson, out var allowed) && allowed.Count > 0
                    && !string.IsNullOrEmpty(v)
                    && !allowed.Contains(v, StringComparer.Ordinal))
                    return new FormSubmissionResult(false, $"“{field.DefaultLabel ?? field.FieldKey}” için geçersiz seçim.");
            }

            if (field.FieldType == FormFieldType.File)
                continue;

            payload[field.FieldKey] = v;
        }

        var json = JsonSerializer.Serialize(payload);
        _db.FormSubmissions.Add(new FormSubmission
        {
            FormDefinitionId = form.Id,
            PayloadJson = json,
            SubmitterIp = submitterIp
        });
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Form submission saved: {FormKey} company {Company}", key, companyCode);

        if (form.SendEmailOnSubmission)
        {
            var recipients = ParseEmailList(form.NotifyEmails);
            if (recipients.Count == 0)
            {
                var fallback = await _db.MailSettings.AsNoTracking()
                    .Where(m => m.CompanyId == companyId)
                    .Select(m => m.FormRecipientEmails)
                    .FirstOrDefaultAsync(cancellationToken);
                recipients = ParseEmailList(fallback);
            }

            if (recipients.Count > 0)
            {
                var body = BuildEmailBody(form.Name, form.Key, ordered, payload, submitterIp);
                var subject = $"[{form.Name}] Yeni form gönderimi";
                var mailResult = await _mail.TrySendPlainAsync(companyId.Value, recipients, subject, body, cancellationToken);
                if (!mailResult.Ok)
                    _logger.LogWarning("Form bildirim e-postası gönderilemedi: {Error}", mailResult.Error);
            }
            else
                _logger.LogWarning("Form {FormKey} için e-posta bildirimi açık ancak alıcı tanımlı değil.", key);
        }

        return new FormSubmissionResult(true, null);
    }

    private static string BuildEmailBody(
        string formName,
        string formKey,
        List<FormField> orderedFields,
        Dictionary<string, string> payload,
        string? submitterIp)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Form: {formName} ({formKey})");
        sb.AppendLine($"Zaman (UTC): {DateTime.UtcNow:O}");
        sb.AppendLine($"IP: {submitterIp ?? "—"}");
        sb.AppendLine();
        foreach (var field in orderedFields)
        {
            if (field.FieldType == FormFieldType.File)
                continue;
            payload.TryGetValue(field.FieldKey, out var val);
            val ??= string.Empty;
            var label = field.DefaultLabel ?? field.FieldKey;
            sb.AppendLine($"{label} ({field.FieldKey}): {val}");
        }

        return sb.ToString();
    }

    private static List<string> ParseEmailList(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
            return new List<string>();
        return csv
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => s.Contains('@', StringComparison.Ordinal) && !s.Contains(' ', StringComparison.Ordinal))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool TryParseOptions(string? json, out List<string> options)
    {
        options = new List<string>();
        if (string.IsNullOrWhiteSpace(json))
            return false;
        try
        {
            var list = JsonSerializer.Deserialize<List<string>>(json);
            if (list is null)
                return false;
            options = list
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToList();
            return options.Count > 0;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
