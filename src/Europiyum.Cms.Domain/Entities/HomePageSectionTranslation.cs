namespace Europiyum.Cms.Domain.Entities;

public class HomePageSectionTranslation
{
    public int Id { get; set; }

    public int HomePageSectionId { get; set; }

    public HomePageSection HomePageSection { get; set; } = null!;

    public int LanguageId { get; set; }

    public Language Language { get; set; } = null!;

    public string? Title { get; set; }

    public string? Subtitle { get; set; }

    public string? BodyHtml { get; set; }

    public string? JsonPayload { get; set; }
}
