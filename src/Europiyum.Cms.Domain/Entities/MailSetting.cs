using System.ComponentModel.DataAnnotations;
using Europiyum.Cms.Domain.Common;

namespace Europiyum.Cms.Domain.Entities;

public class MailSetting : AuditableEntity
{
    public int CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    public string SmtpHost { get; set; } = string.Empty;

    public int SmtpPort { get; set; } = 587;

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public bool UseSsl { get; set; } = true;

    public string SenderEmail { get; set; } = string.Empty;

    public string SenderName { get; set; } = string.Empty;

    /// <summary>Form bildirimleri için varsayılan alıcılar (virgül veya noktalı virgül). Form tanımında özel liste yoksa kullanılır.</summary>
    [Display(Name = "Form bildirim alıcıları (varsayılan)")]
    public string? FormRecipientEmails { get; set; }
}
