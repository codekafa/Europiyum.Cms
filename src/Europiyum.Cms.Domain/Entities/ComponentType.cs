using Europiyum.Cms.Domain.Common;

namespace Europiyum.Cms.Domain.Entities;

public class ComponentType : AuditableEntity
{
    public string Key { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<ComponentItem> Items { get; set; } = new List<ComponentItem>();
}
