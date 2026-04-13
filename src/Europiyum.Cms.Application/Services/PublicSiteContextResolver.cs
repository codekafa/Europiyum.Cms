using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Europiyum.Cms.Application.Services;

/// <summary>Resolves active company and content language for public sites (shared by menu and page services).</summary>
public static class PublicSiteContextResolver
{
    public sealed record Result(Company Company, Language Language, string LanguageCode);

    public static async Task<Result?> TryResolveAsync(
        CmsDbContext db,
        string companyCode,
        string? languageCode,
        CancellationToken cancellationToken = default)
    {
        var company = await db.Companies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == companyCode && c.IsActive, cancellationToken);
        if (company is null)
            return null;

        var langCode = string.IsNullOrWhiteSpace(languageCode) ? "tr" : languageCode.Trim();
        var language = await db.Languages.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Code == langCode && l.IsActive, cancellationToken);
        if (language is null)
        {
            language = await db.Languages.AsNoTracking()
                .FirstAsync(l => l.Id == company.DefaultLanguageId, cancellationToken);
            langCode = language.Code;
        }

        return new Result(company, language, langCode);
    }
}
