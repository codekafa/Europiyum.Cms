using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Europiyum.Cms.Application.Services;

public class CompanyAdminService
{
    private readonly CmsDbContext _db;

    public CompanyAdminService(CmsDbContext db) => _db = db;

    public async Task<IReadOnlyList<CompanyListItemVm>> ListAsync(CancellationToken cancellationToken = default) =>
        await _db.Companies.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CompanyListItemVm
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                IsActive = c.IsActive,
                HomepageVariantKey = c.HomepageVariantKey,
                PrimaryDomain = c.PrimaryDomain
            })
            .ToListAsync(cancellationToken);

    public async Task<CompanyEditVm?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var c = await _db.Companies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (c is null)
            return null;

        return new CompanyEditVm
        {
            Id = c.Id,
            Name = c.Name,
            Code = c.Code,
            Slug = c.Slug,
            IsActive = c.IsActive,
            PrimaryDomain = c.PrimaryDomain,
            HomepageVariantKey = c.HomepageVariantKey,
            DefaultLanguageId = c.DefaultLanguageId
        };
    }

    public async Task SaveAsync(CompanyEditVm vm, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Companies.FirstOrDefaultAsync(c => c.Id == vm.Id, cancellationToken)
            ?? throw new InvalidOperationException("Company not found.");

        var defaultOk = await _db.CompanyLanguages.AnyAsync(
            cl => cl.CompanyId == vm.Id && cl.LanguageId == vm.DefaultLanguageId && cl.IsEnabled,
            cancellationToken);
        if (!defaultOk)
            throw new InvalidOperationException("Varsayılan dil, şirketin aktif dilleri arasında olmalıdır.");

        entity.Name = vm.Name;
        entity.Code = vm.Code;
        entity.Slug = vm.Slug;
        entity.IsActive = vm.IsActive;
        entity.PrimaryDomain = string.IsNullOrWhiteSpace(vm.PrimaryDomain) ? null : vm.PrimaryDomain.Trim();
        entity.HomepageVariantKey = vm.HomepageVariantKey.Trim();
        entity.DefaultLanguageId = vm.DefaultLanguageId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }
}
