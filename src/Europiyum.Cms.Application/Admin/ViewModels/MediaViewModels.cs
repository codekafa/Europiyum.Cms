namespace Europiyum.Cms.Application.Admin.ViewModels;

public class MediaListItemVm
{
    public int Id { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string? AltText { get; set; }

    public string PublicUrl => "/media/" + RelativePath.Replace('\\', '/');
}
