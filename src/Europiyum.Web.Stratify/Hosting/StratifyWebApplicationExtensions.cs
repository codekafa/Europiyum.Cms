using Europiyum.Cms.Application.Configuration;
using Europiyum.Cms.Application.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Europiyum.Web.Stratify;

public static class StratifyWebApplicationExtensions
{
    /// <summary>
    /// Kök adres (/) ve kültürsüz Home/Index isteklerini CMS’teki varsayılan dile yönlendirir;
    /// eski /Home/Page?slug= adreslerini /{culture}/{slug} biçimine çevirir.
    /// </summary>
    public static WebApplication UseStratifyPublicSitePipeline(this WebApplication app)
    {
        app.UseStratifyDefaultCultureRedirect();
        app.UseStratifyLegacyPageRedirect();
        return app;
    }

    /// <summary>/ ve /Home/Index → /{varsayılan-dil} (panelde seçili şirket varsayılan dili).</summary>
    public static WebApplication UseStratifyDefaultCultureRedirect(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            {
                await next();
                return;
            }

            if (!IsCulturelessHomePath(context.Request.Path.Value))
            {
                await next();
                return;
            }

            var siteOpts = context.RequestServices.GetRequiredService<IOptions<CompanySiteOptions>>().Value;
            if (string.IsNullOrWhiteSpace(siteOpts.CompanyCode))
            {
                await next();
                return;
            }

            var langSvc = context.RequestServices.GetRequiredService<ICompanySiteLanguageService>();
            var defaultLang = await langSvc.GetDefaultLanguageCodeAsync(siteOpts.CompanyCode, context.RequestAborted);
            if (string.IsNullOrWhiteSpace(defaultLang))
            {
                await next();
                return;
            }

            var query = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;
            var target = $"/{Uri.EscapeDataString(defaultLang.Trim())}{query}";
            context.Response.Redirect(target);
            return;
        });

        return app;
    }

    /// <summary>Eski /Home/Page?slug=&amp;culture= adreslerini /{culture}/{slug} biçimine kalıcı yönlendirir.</summary>
    public static WebApplication UseStratifyLegacyPageRedirect(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path;
            if (path.Equals("/Home/Page", StringComparison.OrdinalIgnoreCase))
            {
                var slug = context.Request.Query["slug"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(slug))
                {
                    var culture = context.Request.Query["culture"].FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(culture))
                    {
                        var siteOpts = context.RequestServices.GetRequiredService<IOptions<CompanySiteOptions>>().Value;
                        if (!string.IsNullOrWhiteSpace(siteOpts.CompanyCode))
                        {
                            var langSvc = context.RequestServices.GetRequiredService<ICompanySiteLanguageService>();
                            culture = await langSvc.GetDefaultLanguageCodeAsync(siteOpts.CompanyCode, context.RequestAborted);
                        }
                        else
                        {
                            culture = "tr";
                        }
                    }

                    var c = culture.Trim();
                    var s = slug.Trim();
                    var target = $"/{Uri.EscapeDataString(c)}/{Uri.EscapeDataString(s)}";
                    context.Response.Redirect(target, permanent: true);
                    return;
                }
            }

            await next();
        });
        return app;
    }

    private static bool IsCulturelessHomePath(string? path)
    {
        if (string.IsNullOrEmpty(path) || path == "/")
            return true;

        return path.Equals("/Home", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/Home/Index", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>SEO dostu /{culture}/{slug} ve /{culture} (anasayfa) rotalarını kaydeder.</summary>
    public static WebApplication MapStratifyPublicRoutes(this WebApplication app)
    {
        app.MapControllerRoute(
            name: "localizedFormSubmit",
            pattern: "{culture}/forms/{formKey}/submit",
            defaults: new { controller = "Forms", action = "Submit" },
            constraints: new
            {
                culture = new RegexRouteConstraint("^[a-z]{2}(-[a-z]{2,4})?$"),
                formKey = new RegexRouteConstraint("^[a-zA-Z0-9][a-zA-Z0-9\\-_]*$")
            });

        // Önce: içerik sayfası (ör. /tr/hakkimizda)
        app.MapControllerRoute(
            name: "localizedPage",
            pattern: "{culture}/{slug}",
            defaults: new { controller = "Home", action = "Page" },
            constraints: new
            {
                culture = new RegexRouteConstraint("^[a-z]{2}(-[a-z]{2,4})?$"),
                slug = new RegexRouteConstraint("^[a-zA-Z0-9](?:[a-zA-Z0-9\\-_]*[a-zA-Z0-9])?$")
            });

        // /tr → anasayfa (seçili dil)
        app.MapControllerRoute(
            name: "localizedHome",
            pattern: "{culture}",
            defaults: new { controller = "Home", action = "Index" },
            constraints: new { culture = new RegexRouteConstraint("^[a-z]{2}(-[a-z]{2,4})?$") });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        return app;
    }
}
