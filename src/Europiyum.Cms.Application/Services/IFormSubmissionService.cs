namespace Europiyum.Cms.Application.Services;

public interface IFormSubmissionService
{
    Task<FormSubmissionResult> TrySubmitAsync(
        string companyCode,
        string formKey,
        IReadOnlyDictionary<string, string> fields,
        string? submitterIp,
        CancellationToken cancellationToken = default);
}

public sealed record FormSubmissionResult(bool Ok, string? ErrorMessage);
