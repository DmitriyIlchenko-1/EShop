namespace EShop.Core.Platform.Logging.Services;

public static class KnownActivityFormats
{
    public const string Login = "{0}-{1} logged in.";
    public const string ExternalLogin = "{0}-{1} logged in using external auth service: {2}";
    public const string Logout = "{0}-{1} logged out.";
    public const string ExternalLoginFailed = "External login failed. Service: {0}, Error: {1}";
}