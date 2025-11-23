namespace EShop.Core.Platform.Logging.Services;

public static class KnownActivityLogType
{
    private const string PublicActivity = "PublicActivity.";
    public const string ViewProduct = PublicActivity + "ViewProduct";
    
    
    
    public const string Login = PublicActivity + "Login";
    public const string Logout = PublicActivity + "Logout";
    public const string ExternalLoginFailed = PublicActivity + "ExternalLoginFailed";
}