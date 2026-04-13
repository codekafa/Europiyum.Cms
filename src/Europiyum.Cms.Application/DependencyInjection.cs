using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Services;
using Europiyum.Cms.Application.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Europiyum.Cms.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCmsApplication(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<IMediaFileStorage, LocalMediaFileStorage>();
        services.AddScoped<IPublicContentService, PublicContentService>();
        services.AddScoped<CompanyAdminService>();
        services.AddScoped<LanguageAdminService>();
        services.AddScoped<PageAdminService>();
        services.AddScoped<CompanyLanguageAdminService>();
        services.AddScoped<ComponentAdminService>();
        services.AddScoped<HomePageSectionAdminService>();
        services.AddScoped<MediaAdminService>();
        services.AddScoped<PageAttachmentAdminService>();
        services.AddScoped<IPublicMenuService, PublicMenuService>();
        services.AddScoped<IPublicAppearanceService, PublicAppearanceService>();
        services.AddScoped<SiteSettingAdminService>();
        services.AddScoped<MenuItemAdminService>();
        services.AddScoped<FormDefinitionAdminService>();
        services.AddScoped<ISmtpEmailService, SmtpEmailService>();
        services.AddScoped<IFormSubmissionService, FormSubmissionService>();
        services.AddScoped<SeoAdminService>();
        return services;
    }
}
