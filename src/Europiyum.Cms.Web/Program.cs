using Europiyum.Cms.Application;
using Europiyum.Cms.Application.Abstractions;
using Europiyum.Cms.Application.Configuration;
using Microsoft.AspNetCore.Http.Features;
using Europiyum.Cms.Infrastructure;
using Europiyum.Cms.Infrastructure.Persistence;
using Europiyum.Cms.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AdminAuthOptions>(builder.Configuration.GetSection(AdminAuthOptions.SectionName));
builder.Services.Configure<MediaStorageOptions>(builder.Configuration.GetSection(MediaStorageOptions.SectionName));
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "evropiyum.cms.admin";
    options.Cookie.HttpOnly = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/login";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<IAdminWorkspace, AdminWorkspace>();
builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 20 * 1024 * 1024);
builder.Services.AddCmsInfrastructure(builder.Configuration);
builder.Services.AddCmsApplication();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var mediaStorageOptions = app.Services.GetRequiredService<IOptions<MediaStorageOptions>>().Value;
var mediaRequestPath = string.IsNullOrWhiteSpace(mediaStorageOptions.RequestPath) ? "/media" : mediaStorageOptions.RequestPath.Trim();
if (!mediaRequestPath.StartsWith('/'))
    mediaRequestPath = "/" + mediaRequestPath;

var mediaRoot = ResolveMediaRootPath(app.Environment, mediaStorageOptions);
Directory.CreateDirectory(mediaRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaRoot),
    RequestPath = mediaRequestPath
});

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CmsDbContext>();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("Database");
    await db.Database.MigrateAsync();
    await CmsDatabaseSeeder.SeedAsync(db, logger);
}

app.Run();

static string ResolveMediaRootPath(IWebHostEnvironment env, MediaStorageOptions options)
{
    var configured = options.RootPath?.Trim();
    if (string.IsNullOrWhiteSpace(configured))
        return Path.Combine(env.WebRootPath, "media");

    if (Path.IsPathRooted(configured))
        return configured;

    return Path.GetFullPath(Path.Combine(env.ContentRootPath, configured));
}
