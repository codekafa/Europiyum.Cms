using Europiyum.Cms.Domain.Common;

namespace Europiyum.Cms.Domain.Entities;

public class FormSubmission : AuditableEntity
{
    public int FormDefinitionId { get; set; }

    public FormDefinition FormDefinition { get; set; } = null!;

    public string PayloadJson { get; set; } = "{}";

    public string? SubmitterIp { get; set; }
}
