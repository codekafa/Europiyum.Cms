using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Europiyum.Cms.Application.Services;

public class CompanySiteLanguageService : ICompanySiteLanguageService
{
    private const string FallbackLanguageCode = "tr";

    public static string CacheKeyFor(string companyCode) =>
        $"company-default-lang:{(companyCode ?? string.Empty).Trim()}";

    private readonly CmsDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CompanySiteLanguageService> _logger;

    public CompanySiteLanguageService(
        CmsDbContext db,
        IMemoryCache cache,
        ILogger<CompanySiteLanguageService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string> GetDefaultLanguageCodeAsync(string companyCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(companyCode))
            return FallbackLanguageCode;

        var code = companyCode.Trim();
        var cacheKey = CacheKeyFor(code);
        return (await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
            return await LoadDefaultLanguageCodeAsync(code, cancellationToken);
        }))!;
    }

    private async Task<string> LoadDefaultLanguageCodeAsync(string companyCode, CancellationToken cancellationToken)
    {
        var company = await _db.Companies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == companyCode && c.IsActive, cancellationToken);

        if (company is null)
        {
            _logger.LogWarning("Default language: company {Code} not found", companyCode);
            return FallbackLanguageCode;
        }

        var enabled = await _db.CompanyLanguages.AsNoTracking()
            .Where(cl => cl.CompanyId == company.Id && cl.IsEnabled && cl.Language.IsActive)
            .OrderBy(cl => cl.DisplayOrder)
            .ThenBy(cl => cl.LanguageId)
            .Select(cl => new { cl.LanguageId, cl.IsDefault, cl.Language.Code })
            .ToListAsync(cancellationToken);

        if (enabled.Count == 0)
            return FallbackLanguageCode;

        var fromDefaultFlag = enabled.FirstOrDefault(cl => cl.IsDefault || cl.LanguageId == company.DefaultLanguageId);
        if (fromDefaultFlag is not null && !string.IsNullOrWhiteSpace(fromDefaultFlag.Code))
            return fromDefaultFlag.Code.Trim().ToLowerInvariant();

        var fallbackLang = await _db.Languages.AsNoTracking()
            .Where(l => l.Id == company.DefaultLanguageId && l.IsActive)
            .Select(l => l.Code)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(fallbackLang))
            return fallbackLang.Trim().ToLowerInvariant();

        return enabled[0].Code.Trim().ToLowerInvariant();
    }
}
