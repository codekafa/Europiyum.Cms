using Europiyum.Cms.Domain.Entities;
using Europiyum.Cms.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Europiyum.Cms.Infrastructure.Persistence;

public static class CmsDatabaseSeeder
{
    public static async Task SeedAsync(CmsDbContext db, ILogger logger, CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Languages.AnyAsync(cancellationToken))
            return;

        logger.LogInformation("Seeding CMS baseline data (languages, component types, companies).");

        var tr = new Language { Code = "tr", Name = "Türkçe", IsRtl = false, IsActive = true };
        var en = new Language { Code = "en", Name = "English", IsRtl = false, IsActive = true };
        db.Languages.AddRange(tr, en);
        await db.SaveChangesAsync(cancellationToken);

        var types = new[]
        {
            new ComponentType { Key = "hero", DisplayName = "Hero", IsActive = true },
            new ComponentType { Key = "slider", DisplayName = "Slider", IsActive = true },
            new ComponentType { Key = "text-block", DisplayName = "Text block", IsActive = true },
            new ComponentType { Key = "image-block", DisplayName = "Image block", IsActive = true },
            new ComponentType { Key = "cta", DisplayName = "CTA", IsActive = true },
            new ComponentType { Key = "faq", DisplayName = "FAQ", IsActive = true },
            new ComponentType { Key = "rich-text", DisplayName = "Rich text", IsActive = true }
        };
        db.ComponentTypes.AddRange(types);
        await db.SaveChangesAsync(cancellationToken);

        var companies = new[]
        {
            NewCompany("Partexo", "partexo", "partexo", "index", tr.Id),
            NewCompany("Vera Otomotiv", "veraotomotiv", "vera-otomotiv", "index-3", tr.Id),
            NewCompany("TN Motomotiv", "tnmotomotiv", "tn-motomotiv", "index-5", tr.Id),
            NewCompany("Rutenyum Solutions", "rutenyumsolutions", "rutenyum-solutions", "index-7", tr.Id)
        };
        db.Companies.AddRange(companies);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var c in companies)
        {
            db.CompanyLanguages.AddRange(
                new CompanyLanguage { CompanyId = c.Id, LanguageId = tr.Id, IsDefault = true, IsEnabled = true, DisplayOrder = 0 },
                new CompanyLanguage { CompanyId = c.Id, LanguageId = en.Id, IsDefault = false, IsEnabled = true, DisplayOrder = 1 });

            var homePage = new Page
            {
                CompanyId = c.Id,
                PageType = PageType.Home,
                Slug = "",
                TemplateKey = c.HomepageVariantKey,
                SortOrder = 0,
                IsActive = true
            };
            db.Pages.Add(homePage);
        }

        await db.SaveChangesAsync(cancellationToken);

        foreach (var company in await db.Companies.Include(x => x.Pages).ToListAsync(cancellationToken))
        {
            var home = company.Pages.First(p => p.PageType == PageType.Home);
            db.PageTranslations.AddRange(
                new PageTranslation { PageId = home.Id, LanguageId = tr.Id, Title = company.Name, Slug = "" },
                new PageTranslation { PageId = home.Id, LanguageId = en.Id, Title = company.Name, Slug = "" });

            db.HomePageSections.Add(new HomePageSection
            {
                CompanyId = company.Id,
                SectionKey = "hero",
                SortOrder = 0,
                IsActive = true
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        var heroSections = await db.HomePageSections.Where(x => x.SectionKey == "hero").ToListAsync(cancellationToken);
        foreach (var s in heroSections)
        {
            var company = companies.First(c => c.Id == s.CompanyId);
            db.HomePageSectionTranslations.AddRange(
                new HomePageSectionTranslation
                {
                    HomePageSectionId = s.Id,
                    LanguageId = tr.Id,
                    Title = company.Name,
                    Subtitle = "Kurumsal çözümler",
                    BodyHtml = null
                },
                new HomePageSectionTranslation
                {
                    HomePageSectionId = s.Id,
                    LanguageId = en.Id,
                    Title = company.Name,
                    Subtitle = "Corporate solutions",
                    BodyHtml = null
                });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static Company NewCompany(string name, string code, string slug, string homepageKey, int defaultLangId) =>
        new()
        {
            Name = name,
            Code = code,
            Slug = slug,
            IsActive = true,
            PrimaryDomain = null,
            DefaultLanguageId = defaultLangId,
            HomepageVariantKey = homepageKey
        };
}
