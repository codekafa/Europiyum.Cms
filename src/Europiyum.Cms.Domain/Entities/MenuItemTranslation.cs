namespace Europiyum.Cms.Domain.Entities;

public class MenuItemTranslation
{
    public int Id { get; set; }

    public int MenuItemId { get; set; }

    public MenuItem MenuItem { get; set; } = null!;

    public int LanguageId { get; set; }

    public Language Language { get; set; } = null!;

    public string Label { get; set; } = string.Empty;
}
