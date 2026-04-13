using Europiyum.Cms.Application.Admin.ViewModels;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Europiyum.Cms.Application.Services;

public class LanguageAdminService
{
    private readonly CmsDbContext _db;

    public LanguageAdminService(CmsDbContext db) => _db = db;

    public async Task<IReadOnlyList<LanguageVm>> ListAsync(CancellationToken cancellationToken = default) =>
        await _db.Languages.AsNoTracking()
            .OrderBy(l => l.Code)
            .Select(l => new LanguageVm
            {
                Id = l.Id,
                Code = l.Code,
                Name = l.Name,
                IsRtl = l.IsRtl,
                IsActive = l.IsActive
            })
            .ToListAsync(cancellationToken);
}
