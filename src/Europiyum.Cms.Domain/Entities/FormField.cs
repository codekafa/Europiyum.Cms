using Europiyum.Cms.Domain.Common;
using Europiyum.Cms.Domain.Enums;

namespace Europiyum.Cms.Domain.Entities;

public class FormField : AuditableEntity
{
    public int FormDefinitionId { get; set; }

    public FormDefinition FormDefinition { get; set; } = null!;

    public string FieldKey { get; set; } = string.Empty;

    public FormFieldType FieldType { get; set; }

    public int SortOrder { get; set; }

    public bool IsRequired { get; set; }

    public string? DefaultLabel { get; set; }

    public string? OptionsJson { get; set; }
}
