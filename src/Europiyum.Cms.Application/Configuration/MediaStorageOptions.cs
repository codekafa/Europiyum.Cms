namespace Europiyum.Cms.Application.Configuration;

/// <summary>
/// Physical storage settings for uploaded media files.
/// </summary>
public class MediaStorageOptions
{
    public const string SectionName = "MediaStorage";

    /// <summary>
    /// Absolute or content-root relative physical directory for media files.
    /// If empty, falls back to wwwroot/media.
    /// </summary>
    public string? RootPath { get; set; }

    /// <summary>
    /// Public request path used to expose media files.
    /// </summary>
    public string RequestPath { get; set; } = "/media";
}

