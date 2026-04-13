using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Europiyum.Cms.Application.Services;

public class SiteSettingAdminService
{
    private readonly CmsDbContext _db;
    private readonly IMemoryCache _cache;

    public SiteSettingAdminService(CmsDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetMapAsync(int companyId, CancellationToken cancellationToken = default) =>
        await _db.SiteSettings.AsNoTracking()
            .Where(s => s.CompanyId == companyId)
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

    public async Task UpsertAsync(int companyId, IReadOnlyDictionary<string, string?> values, CancellationToken cancellationToken = default)
    {
        foreach (var (key, value) in values)
        {
            if (string.IsNullOrWhiteSpace(key))
                continue;

            var k = key.Trim();
            if (k.Length > 128)
                continue;

            var existing = await _db.SiteSettings.FirstOrDefaultAsync(
                s => s.CompanyId == companyId && s.Key == k, cancellationToken);

            if (string.IsNullOrWhiteSpace(value))
            {
                if (existing is not null)
                {
                    _db.SiteSettings.Remove(existing);
                }

                continue;
            }

            if (existing is null)
            {
                _db.SiteSettings.Add(new SiteSetting
                {
                    CompanyId = companyId,
                    Key = k,
                    Value = value.Trim()
                });
            }
            else
            {
                existing.Value = value.Trim();
                existing.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        var code = await _db.Companies.AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => c.Code)
            .FirstAsync(cancellationToken);
        _cache.Remove($"appearance:{code}");
    }
}
