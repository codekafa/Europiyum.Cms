namespace Europiyum.Cms.Application.Abstractions;

/// <summary>
/// Per-session admin context: which Evropiyum subsidiary is being edited.
/// Implemented in the host web app (cookie session).
/// </summary>
public interface IAdminWorkspace
{
    int? SelectedCompanyId { get; }

    void SetCompany(int companyId);

    void ClearCompany();
}
