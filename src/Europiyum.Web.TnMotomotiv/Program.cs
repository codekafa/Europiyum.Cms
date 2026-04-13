using Europiyum.Cms.Application;
using Europiyum.Cms.Application.Configuration;
using Europiyum.Cms.Infrastructure;
using Europiyum.Cms.Infrastructure.Persistence;
using Europiyum.Web.Stratify;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CompanySiteOptions>(builder.Configuration.GetSection(CompanySiteOptions.SectionName));
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
app.UseRouting();
app.UseAuthorization();

app.UseStratifyLegacyPageRedirect();
app.MapStratifyPublicRoutes();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CmsDbContext>();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    await db.Database.MigrateAsync();
    await CmsDatabaseSeeder.SeedAsync(db, loggerFactory.CreateLogger("Database"));
}

app.Run();
