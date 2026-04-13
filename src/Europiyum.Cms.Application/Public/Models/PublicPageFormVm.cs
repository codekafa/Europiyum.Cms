using System.Collections.Generic;
using Europiyum.Cms.Domain.Enums;

namespace Europiyum.Cms.Application.Public.Models;

public class PublicPageFormVm
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public List<PublicPageFormFieldVm> Fields { get; set; } = new();
}

public class PublicPageFormFieldVm
{
    public string FieldKey { get; set; } = string.Empty;

    public FormFieldType FieldType { get; set; }

    public string? Label { get; set; }

    public bool IsRequired { get; set; }

    /// <summary>Liste ve radyo alanları için seçenek metinleri (gönderilen değer = metin).</summary>
    public List<string> Options { get; set; } = new();
}
