namespace Europiyum.Cms.Application.Public;

/// <summary>Sayfa HTML içeriğinde (veritabanı) kullanılabilecek yer tutucular; sunucu yanıtta değiştirir.</summary>
public static class CmsPageHtmlTokens
{
    /// <summary>Form içinde gizli alan: <c>name="__RequestVerificationToken"</c> <c>value="%%CMS_ANTIFORGERY_TOKEN%%"</c></summary>
    public const string AntiforgeryRequestToken = "%%CMS_ANTIFORGERY_TOKEN%%";

    /// <summary>Örn. <c>action="/%%CMS_LANG%%/forms/iletisim/submit"</c></summary>
    public const string LanguageCode = "%%CMS_LANG%%";
}
