using System.ComponentModel.DataAnnotations;

namespace Europiyum.Cms.Application.Admin.ViewModels;

public class MailSettingsIndexVm
{
    public int Id { get; set; }

    public int CompanyId { get; set; }

    [Required]
    [Display(Name = "SMTP sunucusu")]
    public string SmtpHost { get; set; } = string.Empty;

    [Display(Name = "Port")]
    public int SmtpPort { get; set; } = 587;

    [Display(Name = "Kullanıcı adı")]
    public string? UserName { get; set; }

    [Display(Name = "Parola")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Display(Name = "SSL / TLS kullan")]
    public bool UseSsl { get; set; } = true;

    [Required]
    [Display(Name = "Gönderen e-posta")]
    [EmailAddress]
    public string SenderEmail { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Gönderen adı")]
    public string SenderName { get; set; } = string.Empty;

    [Display(Name = "Form bildirim alıcıları (varsayılan)")]
    public string? FormRecipientEmails { get; set; }

    public List<MailSettingLanguageRecipientRowVm> LanguageRecipients { get; set; } = new();
}

public class MailSettingLanguageRecipientRowVm
{
    public int LanguageId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string LanguageName { get; set; } = string.Empty;

    /// <summary>Bu dil için alıcılar; boşsa varsayılan liste kullanılır.</summary>
    [Display(Name = "Alıcı e-postalar")]
    public string? RecipientEmails { get; set; }
}
