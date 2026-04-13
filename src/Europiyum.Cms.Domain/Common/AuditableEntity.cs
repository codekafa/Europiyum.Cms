namespace Europiyum.Cms.Domain.Common;

public abstract class AuditableEntity
{
    public int Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAt { get; set; }
}
