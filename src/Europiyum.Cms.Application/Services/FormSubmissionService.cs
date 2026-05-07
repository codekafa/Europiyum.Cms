using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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

    private static readonly Regex EmailRegex = new(
        "^[^@\\s]+@[^@\\s]+\\.[^@\\s]+$",
        RegexOptions.Compiled);

    /// <summary>Bir alan için kabul edilen maksimum karakter (DOS koruması).</summary>
    private const int MaxFieldLength = 8000;

    /// <summary>Tanımsız form akışında işlenen maksimum farklı alan sayısı.</summary>
    private const int MaxFieldCount = 50;

    /// <summary>Tanımsız akışta otomatik tespit edilen "isim" alan adları (büyük/küçük harf duyarsız).</summary>
    private static readonly string[] CommonNameKeys = { "name", "form_name", "ad", "isim", "fullname", "full_name" };

    /// <summary>Tanımsız akışta otomatik tespit edilen "email" alan adları.</summary>
    private static readonly string[] CommonEmailKeys = { "email", "form_email", "eposta", "e_posta" };

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
        var key = (formKey ?? string.Empty).Trim();
        if (key.Length == 0)
            return new FormSubmissionResult(false, "Geçersiz form.");
        if (key.Length > 100)
            key = key.Substring(0, 100);

        var company = await _db.Companies.AsNoTracking()
            .Where(c => c.Code == companyCode && c.IsActive)
            .Select(c => new { c.Id, c.Name })
            .FirstOrDefaultAsync(cancellationToken);
        if (company is null)
            return new FormSubmissionResult(false, "Şirket bulunamadı.");

        foreach (var trap in HoneypotFieldNames)
        {
            if (fields.TryGetValue(trap, out var t) && !string.IsNullOrWhiteSpace(t))
                return new FormSubmissionResult(false, "İşlem tamamlanamadı.");
        }

        var definition = await _db.FormDefinitions.AsNoTracking()
            .Include(f => f.Fields)
            .FirstOrDefaultAsync(
                f => f.CompanyId == company.Id && f.Key == key && f.IsActive,
                cancellationToken);

        Dictionary<string, string> payload;
        List<FormField>? orderedFields = null;
        string formName;
        bool sendEmailRequested;
        string? definedNotifyEmails = null;

        if (definition is not null)
        {
            formName = definition.Name;
            sendEmailRequested = definition.SendEmailOnSubmission;
            definedNotifyEmails = definition.NotifyEmails;
            orderedFields = definition.Fields.OrderBy(f => f.SortOrder).ThenBy(f => f.Id).ToList();

            var validation = ValidateAgainstDefinition(orderedFields, fields, out payload);
            if (!validation.Ok)
                return validation;
        }
        else
        {
            formName = key;
            sendEmailRequested = true;
            payload = ExtractRawPayload(fields);
            if (payload.Count == 0)
                return new FormSubmissionResult(false, "Form alanları boş.");
        }

        _db.FormSubmissions.Add(new FormSubmission
        {
            CompanyId = company.Id,
            FormDefinitionId = definition?.Id,
            FormKey = key,
            PayloadJson = JsonSerializer.Serialize(payload),
            SubmitterIp = submitterIp
        });
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Form submission saved: {FormKey} company {Company} (definition={Defined})",
            key, companyCode, definition is not null);

        if (sendEmailRequested)
            await TrySendNotificationAsync(company.Id, formName, key, definition, orderedFields, payload, definedNotifyEmails, submitterIp, cancellationToken);

        return new FormSubmissionResult(true, null);
    }

    private static FormSubmissionResult ValidateAgainstDefinition(
        List<FormField> orderedFields,
        IReadOnlyDictionary<string, string> fields,
        out Dictionary<string, string> payload)
    {
        payload = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in orderedFields)
        {
            fields.TryGetValue(field.FieldKey, out var raw);
            raw ??= string.Empty;
            var v = raw.Trim();
            if (v.Length > MaxFieldLength)
                v = v.Substring(0, MaxFieldLength);

            if (field.IsRequired && string.IsNullOrEmpty(v))
                return new FormSubmissionResult(false, $"\u201C{field.DefaultLabel ?? field.FieldKey}\u201D zorunludur.");

            if (field.FieldType == FormFieldType.Email && !string.IsNullOrEmpty(v) && !EmailRegex.IsMatch(v))
                return new FormSubmissionResult(false, "Geçerli bir e-posta girin.");

            if (field.FieldType is FormFieldType.Select or FormFieldType.Radio)
            {
                if (TryParseOptions(field.OptionsJson, out var allowed) && allowed.Count > 0
                    && !string.IsNullOrEmpty(v)
                    && !allowed.Contains(v, StringComparer.Ordinal))
                    return new FormSubmissionResult(false, $"\u201C{field.DefaultLabel ?? field.FieldKey}\u201D için geçersiz seçim.");
            }

            if (field.FieldType == FormFieldType.File)
                continue;

            payload[field.FieldKey] = v;
        }

        return new FormSubmissionResult(true, null);
    }

    /// <summary>Tanımsız form için: sistem alanlarını filtreleyip gelen tüm key/value çiftlerini alır.</summary>
    private static Dictionary<string, string> ExtractRawPayload(IReadOnlyDictionary<string, string> fields)
    {
        var payload = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var honeypotSet = new HashSet<string>(HoneypotFieldNames, StringComparer.OrdinalIgnoreCase);

        foreach (var (k, v) in fields)
        {
            if (string.IsNullOrWhiteSpace(k)) continue;
            if (k.StartsWith("__", StringComparison.Ordinal)) continue;
            if (honeypotSet.Contains(k)) continue;
            if (payload.Count >= MaxFieldCount) break;

            var val = (v ?? string.Empty).Trim();
            if (val.Length > MaxFieldLength)
                val = val.Substring(0, MaxFieldLength);

            payload[k] = val;
        }

        return payload;
    }

    private async Task TrySendNotificationAsync(
        int companyId,
        string formName,
        string formKey,
        FormDefinition? definition,
        List<FormField>? orderedFields,
        Dictionary<string, string> payload,
        string? definedNotifyEmails,
        string? submitterIp,
        CancellationToken cancellationToken)
    {
        var recipients = ParseEmailList(definedNotifyEmails);
        if (recipients.Count == 0)
        {
            var fallback = await _db.MailSettings.AsNoTracking()
                .Where(m => m.CompanyId == companyId)
                .Select(m => m.FormRecipientEmails)
                .FirstOrDefaultAsync(cancellationToken);
            recipients = ParseEmailList(fallback);
        }

        if (recipients.Count == 0)
        {
            _logger.LogWarning("Form {FormKey} için alıcı tanımlı değil; sadece veritabanına kaydedildi.", formKey);
            return;
        }

        var senderHint = ExtractSenderHint(payload);
        var subject = string.IsNullOrEmpty(senderHint)
            ? $"[{formName}] Yeni form gönderimi"
            : $"[{formName}] Yeni form gönderimi - {senderHint}";

        var body = definition is not null && orderedFields is not null
            ? BuildEmailBodyDefined(formName, formKey, orderedFields, payload, submitterIp)
            : BuildEmailBodyGeneric(formName, formKey, payload, submitterIp);

        var mailResult = await _mail.TrySendPlainAsync(companyId, recipients, subject, body, cancellationToken);
        if (!mailResult.Ok)
            _logger.LogWarning("Form bildirim e-postası gönderilemedi: {Error}", mailResult.Error);
    }

    private static string ExtractSenderHint(Dictionary<string, string> payload)
    {
        foreach (var k in CommonNameKeys)
        {
            if (payload.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v))
                return v.Length > 80 ? v.Substring(0, 80) : v;
        }
        foreach (var k in CommonEmailKeys)
        {
            if (payload.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v))
                return v.Length > 80 ? v.Substring(0, 80) : v;
        }
        return string.Empty;
    }

    private static string BuildEmailBodyDefined(
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

    private static string BuildEmailBodyGeneric(
        string formName,
        string formKey,
        Dictionary<string, string> payload,
        string? submitterIp)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Form: {formName}");
        sb.AppendLine($"Anahtar: {formKey}");
        sb.AppendLine($"Zaman (UTC): {DateTime.UtcNow:O}");
        sb.AppendLine($"IP: {submitterIp ?? "—"}");
        sb.AppendLine();
        sb.AppendLine("Gönderilen alanlar:");
        sb.AppendLine(new string('-', 32));
        foreach (var (k, v) in payload)
        {
            sb.Append(PrettyLabel(k));
            sb.Append(" (");
            sb.Append(k);
            sb.Append("): ");
            sb.AppendLine(v);
        }
        return sb.ToString();
    }

    /// <summary>"form_email" → "Form Email" gibi okunaklı etikete çevirir.</summary>
    private static string PrettyLabel(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return key;
        var s = key.Replace('_', ' ').Replace('-', ' ').Trim();
        if (s.Length == 0)
            return key;
        var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            var p = parts[i];
            parts[i] = char.ToUpperInvariant(p[0]) + (p.Length > 1 ? p.Substring(1).ToLowerInvariant() : string.Empty);
        }
        return string.Join(' ', parts);
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
