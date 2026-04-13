namespace Europiyum.Cms.Application.Admin;

/// <summary>Admin bileşen oluşturma ekranında gösterilecek tip açıklamaları (veritabanı Key ile eşlenir).</summary>
public static class ComponentTypeHelpTexts
{
    private static readonly Dictionary<string, string> ByKey = new(StringComparer.OrdinalIgnoreCase)
    {
        ["hero"] = "Üst vitrin alanı: genelde büyük başlık, kısa metin ve isteğe bağlı buton. Anasayfa veya iç sayfa hero şablonlarında kullanılır.",
        ["slider"] = "Birden fazla slayt/kart içeren kaydırmalı alan. Görseller ve metinler çeviri veya JSON yükü ile beslenir (tema şablonuna göre).",
        ["text-block"] = "Başlık + paragraf düzeni; hizmet özeti, kısa açıklama gibi metin odaklı bloklar için.",
        ["image-block"] = "Tek veya birkaç görsel, isteğe bağlı alt yazı. Medya kütüphanesindeki yollarla çalışır.",
        ["cta"] = "Eylem çağrısı: dikkat çeken başlık, kısa metin ve birincil buton (metin + URL alanları).",
        ["faq"] = "Sık sorulan sorular listesi; accordion veya liste olarak temada işlenir.",
        ["rich-text"] = "Serbest HTML gövde; tablo, listeler ve biçimli metin için esnek blok."
    };

    private const string Fallback = "Bu tip, Stratify tema şablonunda kendi HTML düzenine sahiptir. Düzenle ekranındaki alanları doldurun; eksik alanlar boş kalabilir.";

    public static string GetDescription(string? typeKey) =>
        string.IsNullOrWhiteSpace(typeKey) ? Fallback : ByKey.GetValueOrDefault(typeKey.Trim(), Fallback);
}
