namespace Europiyum.Cms.Domain.Entities;

public class ComponentTranslation
{
    public int Id { get; set; }

    public int ComponentItemId { get; set; }

    public ComponentItem ComponentItem { get; set; } = null!;

    public int LanguageId { get; set; }

    public Language Language { get; set; } = null!;

    public string? Title { get; set; }

    public string? Subtitle { get; set; }

    public string? Description { get; set; }

    public string? BodyHtml { get; set; }

    public string? ButtonText { get; set; }

    public string? ButtonUrl { get; set; }

    public string? ExtraJson { get; set; }
}
