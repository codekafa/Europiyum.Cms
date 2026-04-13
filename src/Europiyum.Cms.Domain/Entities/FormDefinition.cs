using Europiyum.Cms.Domain.Common;

namespace Europiyum.Cms.Domain.Entities;

public class FormDefinition : AuditableEntity
{
    public int CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    /// <summary>Gönderimden sonra SMTP ile bildirim e-postası gönderilsin mi (alıcılar NotifyEmails veya şirket varsayılanı).</summary>
    public bool SendEmailOnSubmission { get; set; }

    /// <summary>Form bazlı alıcılar (virgül veya noktalı virgül). Boşsa şirket mail ayarındaki varsayılan kullanılır.</summary>
    public string? NotifyEmails { get; set; }

    public ICollection<FormField> Fields { get; set; } = new List<FormField>();

    public ICollection<FormSubmission> Submissions { get; set; } = new List<FormSubmission>();
}
