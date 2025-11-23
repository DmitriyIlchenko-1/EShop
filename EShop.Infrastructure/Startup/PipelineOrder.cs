namespace EShop.Infrastructure.Startup;

public static class PipelineOrder
{
    public const int First = -1000;
    public const int Early = -500;
    public const int Default = 0;
    public const int Late = 500;
    public const int Last = 1000;


    public const int BeforeExceptionHandlerMiddleware = -980;
    public const int ExceptionHandlerMiddleware = -960;
    public const int AfterExceptionHandlerMiddleware = -940;
    
    public const int BeforeStaticFilesMiddleware = -880;
    public const int StaticFilesMiddleware = -860;
    public const int AfterStaticFilesMiddleware = -840;
    
    public const int BeforeRoutingMiddleware = -780;
    public const int RoutingMiddleware = -760;
    public const int AfterRoutingMiddleware = -740;
    
    public const int BeforeAuthMiddleware = -780;
    public const int AuthMiddleware = -760;
    public const int AfterAuthMiddleware = -740;
    
}