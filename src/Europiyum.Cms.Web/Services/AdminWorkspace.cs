using Europiyum.Cms.Application.Abstractions;

namespace Europiyum.Cms.Web.Services;

public class AdminWorkspace : IAdminWorkspace
{
    private readonly IHttpContextAccessor _http;
    private const string SessionKey = "cms.selectedCompanyId";

    public AdminWorkspace(IHttpContextAccessor http) => _http = http;

    public int? SelectedCompanyId
    {
        get
        {
            var ctx = _http.HttpContext;
            if (ctx?.Session is null)
                return null;
            var v = ctx.Session.GetInt32(SessionKey);
            return v;
        }
    }

    public void SetCompany(int companyId)
    {
        var ctx = _http.HttpContext ?? throw new InvalidOperationException("No HTTP context.");
        ctx.Session.SetInt32(SessionKey, companyId);
    }

    public void ClearCompany()
    {
        var ctx = _http.HttpContext ?? throw new InvalidOperationException("No HTTP context.");
        ctx.Session.Remove(SessionKey);
    }
}
