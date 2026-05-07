using Europiyum.Cms.Domain.Common;

namespace Europiyum.Cms.Domain.Entities;

public class FormSubmission : AuditableEntity
{
    /// <summary>İlgili tanım yoksa null. Tanım var ise <see cref="FormDefinition.Id"/> referansı.</summary>
    public int? FormDefinitionId { get; set; }

    public FormDefinition? FormDefinition { get; set; }

    /// <summary>Şirket. Tanım olmadan da gönderim kaydını şirkete bağlamak için.</summary>
    public int CompanyId { get; set; }

    public Company Company { get; set; } = null!;

    /// <summary>Submit edilen form key (route'tan). Tanım varsa <see cref="FormDefinition.Key"/> ile aynı.</summary>
    public string FormKey { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = "{}";

    public string? SubmitterIp { get; set; }
}
