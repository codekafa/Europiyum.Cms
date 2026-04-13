using Europiyum.Cms.Domain.Common;

namespace Europiyum.Cms.Domain.Entities;

public class MediaFile : AuditableEntity
{
    public int CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    /// <summary>Path relative to media root (wwwroot/media/{companyCode}/...).</summary>
    public string RelativePath { get; set; } = string.Empty;

    public string? AltText { get; set; }
}
