namespace Europiyum.Cms.Application.Services;

public readonly record struct PageOperationResult(bool Ok, string? Error)
{
    public static PageOperationResult Success() => new(true, null);

    public static PageOperationResult Fail(string message) => new(false, message);
}
