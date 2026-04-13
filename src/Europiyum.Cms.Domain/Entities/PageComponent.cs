namespace Europiyum.Cms.Domain.Entities;

public class PageComponent
{
    public int Id { get; set; }

    public int PageId { get; set; }

    public Page Page { get; set; } = null!;

    public int ComponentItemId { get; set; }

    public ComponentItem ComponentItem { get; set; } = null!;

    public int SortOrder { get; set; }
}
