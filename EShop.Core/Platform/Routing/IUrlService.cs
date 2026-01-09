namespace EShop.Core.Platform.Routing;

public interface IUrlService
{
    Task<string> GetActiveSlugAsync(int entityId, string entityName);
}