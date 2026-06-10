namespace Europiyum.Cms.Application.Public;

/// <summary>Header menüsü ve breadcrumb için dil bazlı ana sayfa metni.</summary>
public static class LocalizedHomeLabel
{
    private static readonly Dictionary<string, string> BuiltIn = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tr"] = "Anasayfa",
        ["en"] = "Home",
        ["de"] = "Startseite",
        ["fr"] = "Accueil",
        ["es"] = "Inicio",
        ["it"] = "Home",
        ["nl"] = "Home",
        ["ar"] = "الرئيسية",
        ["ru"] = "Главная"
    };

    public static string Resolve(
        string? languageCode,
        string? configuredFallback = null,
        IReadOnlyDictionary<string, string>? cmsOverrides = null)
    {
        var code = NormalizeCode(languageCode);
        var baseCode = code.Length >= 2 ? code[..2] : code;

        if (cmsOverrides is not null)
        {
            if (TryGetOverride(cmsOverrides, code, out var custom))
                return custom;
            if (!string.Equals(code, baseCode, StringComparison.Ordinal)
                && TryGetOverride(cmsOverrides, baseCode, out custom))
                return custom;
        }

        if (BuiltIn.TryGetValue(code, out var label))
            return label;
        if (BuiltIn.TryGetValue(baseCode, out label))
            return label;

        if (string.Equals(baseCode, "tr", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(configuredFallback))
            return configuredFallback.Trim();

        return !string.IsNullOrWhiteSpace(configuredFallback) ? configuredFallback.Trim() : "Home";
    }

    private static string NormalizeCode(string? languageCode) =>
        string.IsNullOrWhiteSpace(languageCode) ? "tr" : languageCode.Trim().ToLowerInvariant();

    private static bool TryGetOverride(IReadOnlyDictionary<string, string> dict, string code, out string value)
    {
        if (dict.TryGetValue(code, out var v) && !string.IsNullOrWhiteSpace(v))
        {
            value = v.Trim();
            return true;
        }

        value = string.Empty;
        return false;
    }
}
