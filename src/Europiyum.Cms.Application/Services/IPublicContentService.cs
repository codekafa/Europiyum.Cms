using Europiyum.Cms.Application.Public.Models;

namespace Europiyum.Cms.Application.Services;

public interface IPublicContentService
{
    Task<PublicHomeViewModel?> GetHomeAsync(string companyCode, string? languageCode, CancellationToken cancellationToken = default);

    Task<PublicPageViewModel?> GetPageBySlugAsync(
        string companyCode,
        string slug,
        string? languageCode,
        CancellationToken cancellationToken = default);
}
