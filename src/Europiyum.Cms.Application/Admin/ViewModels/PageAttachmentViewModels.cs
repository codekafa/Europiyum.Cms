using System.ComponentModel.DataAnnotations;
using Europiyum.Cms.Domain.Enums;

namespace Europiyum.Cms.Application.Admin.ViewModels;

public class PageAttachmentListVm
{
    public int PageId { get; set; }

    public int CompanyId { get; set; }

    public string PageTitle { get; set; } = string.Empty;

    public string PageSlug { get; set; } = string.Empty;

    public PageType PageType { get; set; }

    public List<PageAttachmentRowVm> Attachments { get; set; } = new();
}

public class PageAttachmentRowVm
{
    public int PageComponentId { get; set; }

    public int ComponentItemId { get; set; }

    public string ComponentTypeKey { get; set; } = string.Empty;

    public string? TitlePreview { get; set; }

    public int SortOrder { get; set; }
}

public class PageAttachComponentVm
{
    public int PageId { get; set; }

    public int CompanyId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Bileşen seçin")]
    [Display(Name = "Bileşen")]
    public int ComponentItemId { get; set; }

    [Display(Name = "Sıra")]
    public int SortOrder { get; set; }
}
