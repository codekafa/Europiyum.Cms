using System.Text.Json;
using System.Text.RegularExpressions;
using Europiyum.Cms.Application.Admin;
using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Domain.Enums;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Europiyum.Cms.Application.Services;

public class FormDefinitionAdminService
{
    private static readonly Regex FormKeyRegex = new("^[a-z][a-z0-9_-]*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex FieldKeyRegex = new("^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

    private readonly CmsDbContext _db;

    public FormDefinitionAdminService(CmsDbContext db) => _db = db;

    private static string NormalizeFormKey(string? key) => key?.Trim().ToLowerInvariant() ?? "";

    public async Task<(CmsOpResult Result, int NewId)> TryCreateAsync(FormDefinitionCreateVm vm, CancellationToken cancellationToken = default)
    {
        var key = NormalizeFormKey(vm.Key);
        if (string.IsNullOrWhiteSpace(vm.Name))
            return (CmsOpResult.Fail("Ad gerekli."), 0);
        if (string.IsNullOrEmpty(key))
            return (CmsOpResult.Fail("Anahtar gerekli."), 0);
        if (!FormKeyRegex.IsMatch(key))
            return (CmsOpResult.Fail("Anahtar küçük harf ile başlamalı; yalnızca küçük harf, rakam, tire (-) ve alt çizgi (_) kullanılabilir (ör. iletisim-formu)."), 0);

        var exists = await _db.FormDefinitions.AnyAsync(f => f.CompanyId == vm.CompanyId && f.Key == key, cancellationToken);
        if (exists)
            return (CmsOpResult.Fail("Bu anahtar şirkette zaten kullanılıyor."), 0);

        var entity = new FormDefinition
        {
            CompanyId = vm.CompanyId,
            Name = vm.Name.Trim(),
            Key = key,
            IsActive = vm.IsActive
        };
        _db.FormDefinitions.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return (CmsOpResult.Success(), entity.Id);
    }

    public async Task<FormDefinitionEditVm?> GetForEditAsync(int id, int companyId, CancellationToken cancellationToken = default)
    {
        var fd = await _db.FormDefinitions
            .Include(f => f.Fields)
            .FirstOrDefaultAsync(f => f.Id == id && f.CompanyId == companyId, cancellationToken);
        if (fd is null)
            return null;

        var vm = new FormDefinitionEditVm
        {
            Id = fd.Id,
            CompanyId = fd.CompanyId,
            Name = fd.Name,
            Key = fd.Key,
            IsActive = fd.IsActive,
            SendEmailOnSubmission = fd.SendEmailOnSubmission,
            NotifyEmails = fd.NotifyEmails,
            Fields = fd.Fields
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .Select(x => new FormFieldEditVm
                {
                    Id = x.Id,
                    FieldKey = x.FieldKey,
                    FieldType = x.FieldType,
                    IsRequired = x.IsRequired,
                    DefaultLabel = x.DefaultLabel,
                    OptionsText = OptionsJsonToLines(x.OptionsJson)
                })
                .ToList()
        };

        if (vm.Fields.Count == 0)
            vm.Fields.Add(new FormFieldEditVm());

        return vm;
    }

    private static string? OptionsJsonToLines(string? json)
    {
        var list = ParseOptionsList(json);
        return list.Count == 0 ? null : string.Join(Environment.NewLine, list);
    }

    private static List<string> ParseOptionsList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<string>();
        try
        {
            var deserialized = JsonSerializer.Deserialize<List<string>>(json);
            if (deserialized is null)
                return new List<string>();
            return deserialized
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToList();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }

    public async Task<CmsOpResult> TrySaveAsync(FormDefinitionEditVm vm, CancellationToken cancellationToken = default)
    {
        var key = NormalizeFormKey(vm.Key);
        if (string.IsNullOrWhiteSpace(vm.Name))
            return CmsOpResult.Fail("Ad gerekli.");
        if (string.IsNullOrEmpty(key))
            return CmsOpResult.Fail("Anahtar gerekli.");
        if (!FormKeyRegex.IsMatch(key))
            return CmsOpResult.Fail("Anahtar geçersiz (küçük harf ile başlayın; harf, rakam, tire ve alt çizgi).");

        var dupCompany = await _db.FormDefinitions.AnyAsync(
            f => f.CompanyId == vm.CompanyId && f.Key == key && f.Id != vm.Id,
            cancellationToken);
        if (dupCompany)
            return CmsOpResult.Fail("Bu anahtar şirkette başka bir formda kullanılıyor.");

        var fd = await _db.FormDefinitions
            .Include(f => f.Fields)
            .FirstOrDefaultAsync(f => f.Id == vm.Id && f.CompanyId == vm.CompanyId, cancellationToken);
        if (fd is null)
            return CmsOpResult.Fail("Form bulunamadı.");

        var rows = (vm.Fields ?? new List<FormFieldEditVm>())
            .Where(r => !string.IsNullOrWhiteSpace(r.FieldKey))
            .Select(r =>
            {
                r.FieldKey = r.FieldKey.Trim();
                return r;
            })
            .ToList();

        foreach (var row in rows)
        {
            if (!FieldKeyRegex.IsMatch(row.FieldKey))
                return CmsOpResult.Fail($"Alan anahtarı geçersiz: “{row.FieldKey}”. Harf, rakam ve alt çizgi kullanın; rakamla başlamayın.");

            if (row.FieldType is FormFieldType.Select or FormFieldType.Radio)
            {
                var opts = ParseOptionsFromText(row.OptionsText);
                if (opts.Count == 0)
                    return CmsOpResult.Fail($"“{row.FieldKey}” için liste veya radyo alanında en az bir seçenek satırı girin.");
            }
        }

        var duplicateKeys = rows
            .GroupBy(r => r.FieldKey, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicateKeys is not null)
            return CmsOpResult.Fail($"Alan anahtarı yinelenemez: “{duplicateKeys.Key}”.");

        fd.Name = vm.Name.Trim();
        fd.Key = key;
        fd.IsActive = vm.IsActive;
        fd.SendEmailOnSubmission = vm.SendEmailOnSubmission;
        fd.NotifyEmails = string.IsNullOrWhiteSpace(vm.NotifyEmails) ? null : vm.NotifyEmails.Trim();

        var postedExistingIds = rows.Where(r => r.Id > 0).Select(r => r.Id).ToHashSet();
        foreach (var existing in fd.Fields.ToList())
        {
            if (!postedExistingIds.Contains(existing.Id))
                _db.FormFields.Remove(existing);
        }

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var sortOrder = i * 10;
            string? optionsJson = null;
            if (row.FieldType is FormFieldType.Select or FormFieldType.Radio)
                optionsJson = JsonSerializer.Serialize(ParseOptionsFromText(row.OptionsText));

            if (row.Id <= 0)
            {
                _db.FormFields.Add(new FormField
                {
                    FormDefinitionId = fd.Id,
                    FieldKey = row.FieldKey,
                    FieldType = row.FieldType,
                    SortOrder = sortOrder,
                    IsRequired = row.IsRequired,
                    DefaultLabel = string.IsNullOrWhiteSpace(row.DefaultLabel) ? null : row.DefaultLabel.Trim(),
                    OptionsJson = optionsJson
                });
            }
            else
            {
                var entity = fd.Fields.FirstOrDefault(f => f.Id == row.Id);
                if (entity is null)
                    return CmsOpResult.Fail("Kayıt tutarsız: bilinmeyen alan kimliği.");

                entity.FieldKey = row.FieldKey;
                entity.FieldType = row.FieldType;
                entity.SortOrder = sortOrder;
                entity.IsRequired = row.IsRequired;
                entity.DefaultLabel = string.IsNullOrWhiteSpace(row.DefaultLabel) ? null : row.DefaultLabel.Trim();
                entity.OptionsJson = optionsJson;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return CmsOpResult.Success();
    }

    private static List<string> ParseOptionsFromText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();
        return text
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();
    }

    public async Task<CmsOpResult> TryDeleteAsync(int id, int companyId, CancellationToken cancellationToken = default)
    {
        var fd = await _db.FormDefinitions.FirstOrDefaultAsync(f => f.Id == id && f.CompanyId == companyId, cancellationToken);
        if (fd is null)
            return CmsOpResult.Fail("Form bulunamadı.");

        _db.FormDefinitions.Remove(fd);
        await _db.SaveChangesAsync(cancellationToken);
        return CmsOpResult.Success();
    }
}
