using Europiyum.Cms.Domain.Common;
using Europiyum.Cms.Domain.Enums;

namespace Europiyum.Cms.Domain.Entities;

public class Menu : AuditableEntity
{
    public int CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public MenuKind Kind { get; set; }

    public string? Name { get; set; }

    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}
