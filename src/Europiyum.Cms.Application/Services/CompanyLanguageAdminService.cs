using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Europiyum.Cms.Application.Services;

public class CompanyLanguageAdminService
{
    private readonly CmsDbContext _db;

    public CompanyLanguageAdminService(CmsDbContext db) => _db = db;

    public async Task<CompanyLanguagesPageVm?> GetPageAsync(int companyId, CancellationToken cancellationToken = default)
    {
        var company = await _db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken);
        if (company is null)
            return null;

        var rows = await _db.CompanyLanguages.AsNoTracking()
            .Where(cl => cl.CompanyId == companyId)
            .OrderBy(cl => cl.DisplayOrder)
            .ThenBy(cl => cl.LanguageId)
            .Select(cl => new CompanyLanguageRowVm
            {
                LanguageId = cl.LanguageId,
                Code = cl.Language.Code,
                Name = cl.Language.Name,
                IsEnabled = cl.IsEnabled,
                IsDefault = cl.IsDefault,
                DisplayOrder = cl.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        var linked = rows.Select(r => r.LanguageId).ToHashSet();
        var available = await _db.Languages.AsNoTracking()
            .Where(l => l.IsActive && !linked.Contains(l.Id))
            .OrderBy(l => l.Code)
            .Select(l => new LanguageOptionVm { Id = l.Id, Code = l.Code, Name = l.Name })
            .ToListAsync(cancellationToken);

        return new CompanyLanguagesPageVm
        {
            CompanyId = companyId,
            CompanyName = company.Name,
            Rows = rows,
            AvailableLanguages = available
        };
    }

    public async Task<IReadOnlyList<LanguageOptionVm>> GetEnabledLanguageOptionsAsync(int companyId, CancellationToken cancellationToken = default) =>
        await _db.CompanyLanguages.AsNoTracking()
            .Where(cl => cl.CompanyId == companyId && cl.IsEnabled)
            .OrderBy(cl => cl.DisplayOrder)
            .ThenBy(cl => cl.Language.Code)
            .Select(cl => new LanguageOptionVm { Id = cl.LanguageId, Code = cl.Language.Code, Name = cl.Language.Name })
            .ToListAsync(cancellationToken);

    public async Task ApplyBulkUpdateAsync(CompanyLanguagesUpdateVm vm, CancellationToken cancellationToken = default)
    {
        var company = await _db.Companies
            .Include(c => c.CompanyLanguages)
            .FirstOrDefaultAsync(c => c.Id == vm.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException("Company not found.");

        if (vm.Rows.Count == 0)
            throw new InvalidOperationException("Şirkete bağlı dil kaydı yok.");

        var enabledCount = vm.Rows.Count(r => r.IsEnabled);
        if (enabledCount == 0)
            throw new InvalidOperationException("En az bir aktif dil olmalıdır.");

        var defaultRow = vm.Rows.FirstOrDefault(r => r.LanguageId == vm.DefaultLanguageId)
            ?? throw new InvalidOperationException("Geçersiz varsayılan dil.");
        if (!defaultRow.IsEnabled)
            throw new InvalidOperationException("Varsayılan dil aktif olmalıdır.");

        foreach (var input in vm.Rows)
        {
            var cl = company.CompanyLanguages.FirstOrDefault(x => x.LanguageId == input.LanguageId);
            if (cl is null)
                continue;

            cl.IsEnabled = input.IsEnabled;
            cl.DisplayOrder = input.DisplayOrder;
            cl.IsDefault = input.LanguageId == vm.DefaultLanguageId;
        }

        company.DefaultLanguageId = vm.DefaultLanguageId;
        company.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task AddLanguageAsync(int companyId, int languageId, CancellationToken cancellationToken = default)
    {
        var languageExists = await _db.Languages.AnyAsync(l => l.Id == languageId && l.IsActive, cancellationToken);
        if (!languageExists)
            throw new InvalidOperationException("Dil bulunamadı veya pasif.");

        var duplicate = await _db.CompanyLanguages.AnyAsync(
            cl => cl.CompanyId == companyId && cl.LanguageId == languageId, cancellationToken);
        if (duplicate)
            throw new InvalidOperationException("Bu dil zaten ekli.");

        var company = await _db.Companies
            .Include(c => c.CompanyLanguages)
            .FirstAsync(c => c.Id == companyId, cancellationToken);

        var order = company.CompanyLanguages.Count;
        var isFirst = order == 0;

        var row = new CompanyLanguage
        {
            CompanyId = companyId,
            LanguageId = languageId,
            IsEnabled = true,
            IsDefault = isFirst,
            DisplayOrder = order
        };

        if (isFirst)
        {
            foreach (var cl in company.CompanyLanguages)
                cl.IsDefault = false;
            company.DefaultLanguageId = languageId;
        }

        _db.CompanyLanguages.Add(row);
        company.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveLanguageAsync(int companyId, int languageId, CancellationToken cancellationToken = default)
    {
        var company = await _db.Companies
            .Include(c => c.CompanyLanguages)
            .FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken)
            ?? throw new InvalidOperationException("Company not found.");

        if (company.CompanyLanguages.Count <= 1)
            throw new InvalidOperationException("Şirketten en az bir dil atanmış kalmalıdır.");

        var row = company.CompanyLanguages.FirstOrDefault(cl => cl.LanguageId == languageId)
            ?? throw new InvalidOperationException("Dil kaydı yok.");

        var enabledOthers = company.CompanyLanguages.Count(cl => cl.LanguageId != languageId && cl.IsEnabled);
        if (row.IsEnabled && enabledOthers == 0)
            throw new InvalidOperationException("Son aktif dil kaldırılamaz.");

        var wasDefault = row.IsDefault || company.DefaultLanguageId == languageId;

        _db.CompanyLanguages.Remove(row);
        company.CompanyLanguages.Remove(row);

        if (wasDefault)
        {
            var next = company.CompanyLanguages
                .Where(cl => cl.IsEnabled)
                .OrderBy(cl => cl.DisplayOrder)
                .FirstOrDefault()
                ?? company.CompanyLanguages.OrderBy(cl => cl.DisplayOrder).FirstOrDefault();

            if (next is null)
                throw new InvalidOperationException("Şirketin en az bir dili kalmalıdır.");

            foreach (var cl in company.CompanyLanguages)
                cl.IsDefault = cl.LanguageId == next.LanguageId;

            company.DefaultLanguageId = next.LanguageId;
        }

        company.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
