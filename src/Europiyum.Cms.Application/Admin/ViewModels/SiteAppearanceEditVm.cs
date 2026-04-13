using System.ComponentModel.DataAnnotations;

namespace Europiyum.Cms.Application.Admin.ViewModels;

public class SiteAppearanceEditVm
{
    public int CompanyId { get; set; }

    [Display(Name = "Footer giriş metni (HTML)")]
    public string? FooterIntroHtml { get; set; }

    [Display(Name = "Footer ek gövde (HTML bileşen)")]
    public string? FooterBodyHtml { get; set; }

    [Display(Name = "Telif / alt satır (HTML)")]
    public string? FooterCopyrightHtml { get; set; }

    [Display(Name = "Tam footer HTML (doluysa diğer footer alanları ve menüler kullanılmaz)")]
    public string? FooterFullHtml { get; set; }

    [Display(Name = "Offcanvas — mobil menü altı HTML")]
    public string? OffcanvasBelowMenuHtml { get; set; }

    [Display(Name = "Footer logo yolu")]
    public string? BrandingFooterLogoPath { get; set; }

    [Display(Name = "Header — ana logo (göreli veya /media/...)")]
    public string? BrandingHeaderLogoMainPath { get; set; }

    [Display(Name = "Header — açık logo (blur menü)")]
    public string? BrandingHeaderLogoLightPath { get; set; }

    [Display(Name = "Header — iç sayfa (siyah logo)")]
    public string? BrandingHeaderLogoBlackPath { get; set; }

    [Display(Name = "Offcanvas özel logo yolu (boşsa aşağıdaki varyant)")]
    public string? BrandingOffcanvasLogoPath { get; set; }

    [Display(Name = "Favicon yolu")]
    public string? BrandingFaviconPath { get; set; }

    [Display(Name = "Mobil menü (offcanvas) logo")]
    public string OffcanvasLogoVariant { get; set; } = "light";

    [Display(Name = "İletişim e-postası (gösterim)")]
    public string? SiteContactEmail { get; set; }

    [Display(Name = "İletişim telefonu (gösterim)")]
    public string? SiteContactPhone { get; set; }

    [Display(Name = "Head içi script / etiketler (ham HTML)")]
    public string? HeadScriptsHtml { get; set; }

    [Display(Name = "Özel CSS")]
    public string? CustomCss { get; set; }
}
