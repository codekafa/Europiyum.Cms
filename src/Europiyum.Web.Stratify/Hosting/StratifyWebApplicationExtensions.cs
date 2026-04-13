using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;

namespace Europiyum.Web.Stratify;

public static class StratifyWebApplicationExtensions
{
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
                        culture = "tr";

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
