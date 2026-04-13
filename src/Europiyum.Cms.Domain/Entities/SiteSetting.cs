using Europiyum.Cms.Domain.Common;

namespace Europiyum.Cms.Domain.Entities;

public class SiteSetting : AuditableEntity
{
    public int CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
