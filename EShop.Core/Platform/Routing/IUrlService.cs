namespace EShop.Core.Platform.Routing;

public interface IUrlService
{
    Task<string> GetActiveSlugAsync(int entityId, string entityName);
    Task PrefetchUrlRecordsAsync(string entityName, IEnumerable<int> entityIds, bool tracked = false);

    Task<UrlRecordCollection> GetUrlRecordCollectionAsync(string entityName, IEnumerable<int> entityIds,
        bool tracked = false);
}