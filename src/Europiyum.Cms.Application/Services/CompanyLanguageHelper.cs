using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Europiyum.Cms.Application.Services;

public static class CompanyLanguageHelper
{
    public static async Task<List<Language>> GetEnabledLanguagesOrderedAsync(
        CmsDbContext db,
        int companyId,
        CancellationToken cancellationToken = default)
    {
        var langIds = await db.CompanyLanguages.AsNoTracking()
            .Where(cl => cl.CompanyId == companyId && cl.IsEnabled)
            .OrderBy(cl => cl.DisplayOrder)
            .ThenBy(cl => cl.LanguageId)
            .Select(cl => cl.LanguageId)
            .ToListAsync(cancellationToken);

        if (langIds.Count == 0)
            return new List<Language>();

        var langs = await db.Languages.AsNoTracking()
            .Where(l => langIds.Contains(l.Id))
            .ToListAsync(cancellationToken);

        return langIds.Select(id => langs.First(l => l.Id == id)).ToList();
    }
}
