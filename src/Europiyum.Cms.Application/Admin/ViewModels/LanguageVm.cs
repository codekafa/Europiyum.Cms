namespace Europiyum.Cms.Application.Admin.ViewModels;

public class LanguageVm
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsRtl { get; set; }

    public bool IsActive { get; set; }
}
