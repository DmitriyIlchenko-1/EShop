namespace EShop.Core.Platform.Routing;

public interface IUrlService
{
    Task<string> GetActiveSlugAsync(int entityId, string entityName);
    Task PrefetchUrlRecordsAsync(string entityName, int[] entityIds, bool tracked = false);

    Task<UrlRecordCollection> GetUrlRecordCollectionAsync(string entityName, int[] entityIds,
        bool tracked = false);
}