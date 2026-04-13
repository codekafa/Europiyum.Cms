using Europiyum.Cms.Domain.Enums;

namespace Europiyum.Cms.Application.Admin.ViewModels;

public class PageListItemVm
{
    public int Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public PageType PageType { get; set; }

    public string? TemplateKey { get; set; }

    public bool IsActive { get; set; }

    public int SortOrder { get; set; }

    public string? TitlePreview { get; set; }
}
