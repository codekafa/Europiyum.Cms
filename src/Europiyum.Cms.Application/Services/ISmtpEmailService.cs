using Europiyum.Cms.Application.Admin;

namespace Europiyum.Cms.Application.Services;

public interface ISmtpEmailService
{
    /// <summary>Şirket SMTP ayarlarıyla düz metin e-posta gönderir.</summary>
    Task<CmsOpResult> TrySendPlainAsync(
        int companyId,
        IReadOnlyList<string> recipients,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}
