namespace Europiyum.Cms.Application.Admin.ViewModels;

public class CompanyListItemVm
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string HomepageVariantKey { get; set; } = string.Empty;

    public string? PrimaryDomain { get; set; }
}
