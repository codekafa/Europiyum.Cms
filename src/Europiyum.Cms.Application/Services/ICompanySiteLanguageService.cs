namespace Europiyum.Cms.Application.Services;

public interface ICompanySiteLanguageService
{
    /// <summary>CMS’te şirket için tanımlı varsayılan dil kodu (ör. tr, en).</summary>
    Task<string> GetDefaultLanguageCodeAsync(string companyCode, CancellationToken cancellationToken = default);
}
