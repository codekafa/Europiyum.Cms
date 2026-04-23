using System.Text.RegularExpressions;
using Europiyum.Cms.Application.Admin;
using Europiyum.Cms.Application.Configuration;
using Europiyum.Cms.Application.Public.Models;
using Europiyum.Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Europiyum.Cms.Application.Services;

public class PublicAppearanceService : IPublicAppearanceService
{
    private static readonly Regex SafeRelativeStaticPathRegex = new("^[a-zA-Z0-9_./-]+$", RegexOptions.Compiled);
    private static readonly Regex SafeMediaPathRegex = new("^/media/[a-zA-Z0-9_./-]+$", RegexOptions.Compiled);
    private static readonly Regex SafeAbsoluteHttpUrlRegex = new("^https?://[a-zA-Z0-9._~:/?#\\[\\]@!$&'()*+,;=%-]+$", RegexOptions.Compiled);

    private const string StratifyRoot = "/_content/Europiyum.Web.Stratify/stratify/";

    private readonly CmsDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PublicAppearanceService> _logger;
    private readonly CompanySiteOptions _siteOptions;

    public PublicAppearanceService(
        CmsDbContext db,
        IMemoryCache cache,
        ILogger<PublicAppearanceService> logger,
        IOptions<CompanySiteOptions> siteOptions)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
        _siteOptions = siteOptions.Value;
    }

    public async Task<PublicAppearanceSnapshot> GetSnapshotAsync(string companyCode, CancellationToken cancellationToken = default)
    {
        var code = companyCode.Trim();
        var cacheKey = $"appearance:{code}";
        return (await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
            return await LoadSnapshotAsync(code, CancellationToken.None);
        }))!;
    }

    private async Task<PublicAppearanceSnapshot> LoadSnapshotAsync(string companyCode, CancellationToken cancellationToken)
    {
        var companyId = await _db.Companies.AsNoTracking()
            .Where(c => c.Code == companyCode && c.IsActive)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (companyId is null)
        {
            _logger.LogWarning("Appearance: company {Code} not found", companyCode);
            return new PublicAppearanceSnapshot
            {
                FaviconHref = StratifyRoot + "images/favicon.png",
                FooterLogoHref = StratifyRoot + "images/logo/logo-light.png",
                HeaderLogoMainHref = StratifyRoot + "images/logo/logo.png",
                HeaderLogoLightHref = StratifyRoot + "images/logo/logo-light.png",
                HeaderLogoBlackHref = StratifyRoot + "images/logo/logo-black.png",
                OffcanvasLogoHref = StratifyRoot + "images/logo/logo-light.png"
            };
        }

        var rows = await _db.SiteSettings.AsNoTracking()
            .Where(s => s.CompanyId == companyId)
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        string Get(string key, string fallback) =>
            rows.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : fallback;

        var favRaw = Get(SiteSettingKeys.BrandingFaviconPath, "images/favicon.png");
        var footRaw = Get(SiteSettingKeys.BrandingFooterLogoPath, "images/logo/logo-light.png");
        var headerMainRaw = NullIfEmpty(rows, SiteSettingKeys.BrandingHeaderLogoMainPath);
        var headerLightRaw = NullIfEmpty(rows, SiteSettingKeys.BrandingHeaderLogoLightPath);
        var headerBlackRaw = NullIfEmpty(rows, SiteSettingKeys.BrandingHeaderLogoBlackPath);
        var offcanvasPathRaw = NullIfEmpty(rows, SiteSettingKeys.BrandingOffcanvasLogoPath);
        var variant = Get(SiteSettingKeys.OffcanvasLogoVariant, "light").ToLowerInvariant();
        var offcanvasStratifyRel = MapOffcanvasLogo(variant);

        return new PublicAppearanceSnapshot
        {
            FaviconHref = ResolveAssetHref(favRaw, "images/favicon.png", _siteOptions.MediaBaseUrl),
            FooterLogoHref = ResolveAssetHref(footRaw, "images/logo/logo-light.png", _siteOptions.MediaBaseUrl),
            HeaderLogoMainHref = ResolveAssetHref(headerMainRaw, "images/logo/logo.png", _siteOptions.MediaBaseUrl),
            HeaderLogoLightHref = ResolveAssetHref(headerLightRaw, "images/logo/logo-light.png", _siteOptions.MediaBaseUrl),
            HeaderLogoBlackHref = ResolveAssetHref(headerBlackRaw, "images/logo/logo-black.png", _siteOptions.MediaBaseUrl),
            OffcanvasLogoHref = string.IsNullOrWhiteSpace(offcanvasPathRaw)
                ? ResolveAssetHref(offcanvasStratifyRel, "images/logo/logo-light.png", _siteOptions.MediaBaseUrl)
                : ResolveAssetHref(offcanvasPathRaw, "images/logo/logo-light.png", _siteOptions.MediaBaseUrl),
            FooterIntroHtml = NullIfEmpty(rows, SiteSettingKeys.FooterIntroHtml),
            FooterBodyHtml = NullIfEmpty(rows, SiteSettingKeys.FooterBodyHtml),
            FooterCopyrightHtml = NullIfEmpty(rows, SiteSettingKeys.FooterCopyrightHtml),
            FooterFullHtml = NullIfEmpty(rows, SiteSettingKeys.FooterFullHtml),
            OffcanvasBelowMenuHtml = NullIfEmpty(rows, SiteSettingKeys.OffcanvasBelowMenuHtml),
            ContactEmail = NullIfEmpty(rows, SiteSettingKeys.SiteContactEmail),
            ContactPhone = NullIfEmpty(rows, SiteSettingKeys.SiteContactPhone),
            HeadScriptsHtml = NullIfEmpty(rows, SiteSettingKeys.HeadScriptsHtml),
            CustomCss = NullIfEmpty(rows, SiteSettingKeys.CustomCss)
        };
    }

    private static string? NullIfEmpty(Dictionary<string, string> rows, string key) =>
        rows.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v : null;

    private static string MapOffcanvasLogo(string variant) =>
        variant switch
        {
            "dark" => "images/logo/logo-dark.png",
            "black" => "images/logo/logo-black.png",
            "white" => "images/logo/logo-white.png",
            "main" or "default" => "images/logo/logo.png",
            _ => "images/logo/logo-light.png"
        };

    /// <summary>
    /// Tema göreli yol, örn. <c>images/logo.png</c>, veya <c>/media/şirket/dosya.png</c>.
    /// </summary>
    private static string ResolveAssetHref(string? raw, string defaultStratifyRelative, string? mediaBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return StratifyRoot + defaultStratifyRelative.TrimStart('/');

        var s = raw.Trim().Replace('\\', '/');
        if (s.Contains("..", StringComparison.Ordinal))
            return StratifyRoot + defaultStratifyRelative.TrimStart('/');

        if (s.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || s.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return SafeAbsoluteHttpUrlRegex.IsMatch(s)
                ? s
                : StratifyRoot + defaultStratifyRelative.TrimStart('/');
        }

        if (s.StartsWith("/media/", StringComparison.Ordinal))
        {
            if (!SafeMediaPathRegex.IsMatch(s) || s.Length > 512)
                return StratifyRoot + defaultStratifyRelative.TrimStart('/');

            var baseUrl = NormalizeBaseUrl(mediaBaseUrl);
            return baseUrl is null ? s : baseUrl + s;
        }

        var rel = SanitizeStaticPath(s, defaultStratifyRelative);
        return StratifyRoot + rel;
    }

    private static string? NormalizeBaseUrl(string? mediaBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(mediaBaseUrl))
            return null;

        var raw = mediaBaseUrl.Trim().TrimEnd('/');
        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri))
            return null;

        if (!string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            return null;

        return uri.GetLeftPart(UriPartial.Authority);
    }

    private static string SanitizeStaticPath(string path, string fallback)
    {
        if (string.IsNullOrWhiteSpace(path))
            return fallback;

        var p = path.Trim().Replace('\\', '/').TrimStart('/');
        if (p.Contains("..", StringComparison.Ordinal) || p.Contains("://", StringComparison.Ordinal))
            return fallback;

        return SafeRelativeStaticPathRegex.IsMatch(p) ? p : fallback;
    }
}
