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

        Language? language;
        string langCode;

        if (string.IsNullOrWhiteSpace(languageCode))
        {
            language = await db.Languages.AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == company.DefaultLanguageId && l.IsActive, cancellationToken)
                ?? await db.Languages.AsNoTracking()
                    .FirstOrDefaultAsync(l => l.Id == company.DefaultLanguageId, cancellationToken);

            if (language is null)
            {
                var enabledCode = await db.CompanyLanguages.AsNoTracking()
                    .Where(cl => cl.CompanyId == company.Id && cl.IsEnabled && cl.Language.IsActive)
                    .OrderBy(cl => cl.DisplayOrder)
                    .ThenBy(cl => cl.LanguageId)
                    .Select(cl => cl.Language.Code)
                    .FirstOrDefaultAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(enabledCode))
                    return null;

                langCode = enabledCode.Trim();
                language = await db.Languages.AsNoTracking()
                    .FirstAsync(l => l.Code == langCode && l.IsActive, cancellationToken);
            }
            else
            {
                langCode = language.Code;
            }
        }
        else
        {
            langCode = languageCode.Trim();
            language = await db.Languages.AsNoTracking()
                .FirstOrDefaultAsync(l => l.Code == langCode && l.IsActive, cancellationToken)
                ?? await db.Languages.AsNoTracking()
                    .FirstOrDefaultAsync(l => l.Code == langCode, cancellationToken);

            if (language is null)
            {
                language = await db.Languages.AsNoTracking()
                    .FirstAsync(l => l.Id == company.DefaultLanguageId, cancellationToken);
                langCode = language.Code;
            }
        }

        return new Result(company, language, langCode);
    }
}
