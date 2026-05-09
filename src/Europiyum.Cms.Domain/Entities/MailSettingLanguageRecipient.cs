using System.ComponentModel.DataAnnotations;
using Europiyum.Cms.Domain.Common;

namespace Europiyum.Cms.Domain.Entities;

public class MailSettingLanguageRecipient : AuditableEntity
{
    public int MailSettingId { get; set; }

    public MailSetting MailSetting { get; set; } = null!;

    public int LanguageId { get; set; }

    public Language Language { get; set; } = null!;

    /// <summary>Bu dildeki form bildirimi alıcıları (virgül veya noktalı virgül). Boş veya kayıt yoksa varsayılan alıcı listesi kullanılır.</summary>
    [MaxLength(1024)]
    public string? RecipientEmails { get; set; }
}
