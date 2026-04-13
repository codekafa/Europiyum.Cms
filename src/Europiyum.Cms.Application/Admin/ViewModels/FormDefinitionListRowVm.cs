namespace Europiyum.Cms.Application.Admin.ViewModels;

public class FormDefinitionListRowVm
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int FieldCount { get; set; }
}
