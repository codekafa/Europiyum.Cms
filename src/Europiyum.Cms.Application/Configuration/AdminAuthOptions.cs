namespace Europiyum.Cms.Application.Configuration;

public class AdminAuthOptions
{
    public const string SectionName = "AdminAuth";

    public List<AdminUserEntry> Users { get; set; } = new();
}

public class AdminUserEntry
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
}
