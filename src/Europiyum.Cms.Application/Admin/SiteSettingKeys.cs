namespace Europiyum.Cms.Application.Admin;

/// <summary>Şirket başına <see cref="Domain.Entities.SiteSetting"/> anahtarları.</summary>
public static class SiteSettingKeys
{
    public const string FooterIntroHtml = "footer.intro_html";

    public const string FooterBodyHtml = "footer.body_html";

    public const string FooterCopyrightHtml = "footer.copyright_html";

    /// <summary>Stratify göreli yol, örn. images/logo/logo-light.png</summary>
    public const string BrandingFooterLogoPath = "branding.footer_logo_path";

    /// <summary>Stratify göreli yol veya /media/... (yüklenen dosya).</summary>
    public const string BrandingHeaderLogoMainPath = "branding.header_logo_main_path";

    /// <summary>Stratify göreli yol veya /media/...</summary>
    public const string BrandingHeaderLogoLightPath = "branding.header_logo_light_path";

    /// <summary>Stratify göreli yol veya /media/... (iç sayfa header için siyah logo).</summary>
    public const string BrandingHeaderLogoBlackPath = "branding.header_logo_black_path";

    /// <summary>Boşsa <see cref="OffcanvasLogoVariant"/> ile tema dosyası seçilir; doluysa bu yol kullanılır.</summary>
    public const string BrandingOffcanvasLogoPath = "branding.offcanvas_logo_path";

    /// <summary>Stratify göreli yol, örn. images/favicon.png</summary>
    public const string BrandingFaviconPath = "branding.favicon_path";

    /// <summary>light | dark | black | white — offcanvas logosu.</summary>
    public const string OffcanvasLogoVariant = "offcanvas.logo_variant";

    /// <summary>.mobile-menu sonrası offcanvas gövdesine basılan ham HTML (şablon: About + Contact blokları).</summary>
    public const string OffcanvasBelowMenuHtml = "offcanvas.below_menu_html";

    /// <summary>Doluysa tüm footer bu HTML ile değiştirilir (menü sütunları kullanılmaz).</summary>
    public const string FooterFullHtml = "footer.full_html";

    public const string SiteContactEmail = "site.contact_email";

    public const string SiteContactPhone = "site.contact_phone";

    /// <summary>Head içine ham HTML: Analytics, GTM, meta, inline script (güvenilir kaynak).</summary>
    public const string HeadScriptsHtml = "site.head_scripts_html";

    /// <summary>Tema stillerinden sonra eklenen ham CSS (style etiketi içine yazılır).</summary>
    public const string CustomCss = "site.custom_css";
}
