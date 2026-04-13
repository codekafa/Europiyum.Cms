namespace Europiyum.Cms.Application.Admin;

public readonly record struct CmsOpResult(bool Ok, string? Error)
{
    public static CmsOpResult Success() => new(true, null);

    public static CmsOpResult Fail(string message) => new(false, message);
}
