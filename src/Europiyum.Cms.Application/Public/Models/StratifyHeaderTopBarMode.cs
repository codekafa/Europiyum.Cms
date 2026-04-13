namespace Europiyum.Cms.Application.Public.Models;

/// <summary>Üst ince şerit (adres / iletişim) — tema index.html ile hizalama.</summary>
public enum StratifyHeaderTopBarMode
{
    /// <summary>Şerit yok; .header-area için top:0 (boşluk düzeltmesi).</summary>
    None = 0,

    /// <summary>Yalnızca şirket adı satırı.</summary>
    Compact = 1,

    /// <summary>Adres, e-posta ve sosyal ikonlar (index.html ile uyumlu).</summary>
    Full = 2
}
