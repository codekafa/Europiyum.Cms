using Europiyum.Cms.Application.Public.Models;

namespace Europiyum.Cms.Application.Services;

public interface IPublicMenuService
{
    Task<SiteHeaderViewModel> BuildHeaderAsync(
        string companyCode,
        string siteTitle,
        string? languageCode,
        CancellationToken cancellationToken = default);

    Task<SiteFooterViewModel> BuildFooterAsync(
        string companyCode,
        string siteTitle,
        string? languageCode,
        CancellationToken cancellationToken = default);
}
