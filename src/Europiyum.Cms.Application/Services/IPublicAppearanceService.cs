using Europiyum.Cms.Application.Public.Models;

namespace Europiyum.Cms.Application.Services;

public interface IPublicAppearanceService
{
    Task<PublicAppearanceSnapshot> GetSnapshotAsync(string companyCode, CancellationToken cancellationToken = default);
}
