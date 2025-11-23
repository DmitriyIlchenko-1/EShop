namespace EShop.Infrastructure.Http;

public static class CookieNames
{
    public static string Prefix => ".EShop";
    public static string RecentlyViewedProducts => Prefix + ".RecentlyViewedProducts";
    public static string Visitor => Prefix + ".Visitor";
    public static string Identity => Prefix + ".Identity";
    public static string SessionCookie => Prefix + ".Session";
}