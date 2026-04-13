using Europiyum.Cms.Domain.Common;
using Europiyum.Cms.Domain.Enums;

namespace Europiyum.Cms.Domain.Entities;

public class MenuItem : AuditableEntity
{
    public int MenuId { get; set; }

    public Menu Menu { get; set; } = null!;

    public int? ParentMenuItemId { get; set; }

    public MenuItem? Parent { get; set; }

    public ICollection<MenuItem> Children { get; set; } = new List<MenuItem>();

    public int SortOrder { get; set; }

    public MenuLinkType LinkType { get; set; }

    public string? ExternalUrl { get; set; }

    public string? Anchor { get; set; }

    public int? TargetPageId { get; set; }

    public Page? TargetPage { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<MenuItemTranslation> Translations { get; set; } = new List<MenuItemTranslation>();
}
