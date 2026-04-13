using Europiyum.Cms.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Europiyum.Cms.Infrastructure.Persistence;

public class CmsDbContext : DbContext
{
    public CmsDbContext(DbContextOptions<CmsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Language> Languages => Set<Language>();

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<CompanyLanguage> CompanyLanguages => Set<CompanyLanguage>();

    public DbSet<Page> Pages => Set<Page>();

    public DbSet<PageTranslation> PageTranslations => Set<PageTranslation>();

    public DbSet<SeoMetadata> SeoMetadata => Set<SeoMetadata>();

    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();

    public DbSet<ComponentType> ComponentTypes => Set<ComponentType>();

    public DbSet<ComponentItem> ComponentItems => Set<ComponentItem>();

    public DbSet<ComponentTranslation> ComponentTranslations => Set<ComponentTranslation>();

    public DbSet<PageComponent> PageComponents => Set<PageComponent>();

    public DbSet<HomePageSection> HomePageSections => Set<HomePageSection>();

    public DbSet<HomePageSectionTranslation> HomePageSectionTranslations => Set<HomePageSectionTranslation>();

    public DbSet<Menu> Menus => Set<Menu>();

    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    public DbSet<MenuItemTranslation> MenuItemTranslations => Set<MenuItemTranslation>();

    public DbSet<FormDefinition> FormDefinitions => Set<FormDefinition>();

    public DbSet<FormField> FormFields => Set<FormField>();

    public DbSet<FormSubmission> FormSubmissions => Set<FormSubmission>();

    public DbSet<MailSetting> MailSettings => Set<MailSetting>();

    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CmsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
