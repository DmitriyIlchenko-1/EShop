using System.Collections;
using System.Globalization;
using EShop.Core.Data;
using EShop.Core.Platform.Caching;
using EShop.Core.Platform.Routing.Domain;
using EShop.Core.Platform.Routing.Settings;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Platform.Routing;

public class UrlService : IUrlService
{
    private readonly ApplicationDbContext _context;
    private readonly SeoSettings _seoSettings;
    private readonly ICacheManager _cache;
    private readonly IDictionary<string, UrlRecordCollection> _prefetchedUrlSlugCollections;

    private const string URLRECORD_ALL_ACTIVE_KEY = "urlrecond:all-active";

    public UrlService(ApplicationDbContext context, SeoSettings seoSettings, ICacheManager cache)
    {
        _context = context;
        _seoSettings = seoSettings;
        _cache = cache;
        _prefetchedUrlSlugCollections = new Dictionary<string, UrlRecordCollection>();
    }

    public virtual async Task PrefetchUrlRecordsAsync(string entityName, int[] entityIds, bool tracked = false)
    {
        var collection = await GetUrlRecordCollectionAsync(entityName, entityIds, tracked);
        if (_prefetchedUrlSlugCollections.TryGetValue(entityName, out var existingCollection))
        {
            //TODO: continue...
        }
        else
        {
            _prefetchedUrlSlugCollections.Add(entityName, collection);
        }
    }

    public virtual async Task<UrlRecordCollection> GetUrlRecordCollectionAsync(string entityName, int[] entityIds,
        bool tracked = false)
    {
        Guard.NotEmpty(entityName);
        var query = _context
            .UrlRecords.ApplyTracking(tracked)
            .Where(x => x.EntityName == entityName && x.IsActive);

        if (entityIds != null && entityIds.Length != 0)
        {
            query = query.Where(x => entityIds.Contains(x.Id));
        }

        return new UrlRecordCollection(entityName, entityIds, await query.ToListAsync());
    }
    
    public virtual async Task<string> GetActiveSlugAsync(int entityId, string entityName)
    {
        ArgumentNullException.ThrowIfNull(entityName);
        if (TryGetPrefetchedActiveSlug(entityId, entityName, out var slug))
        {
            return slug;
        }

        if (_seoSettings.LoadAllUrlSlugsOnStartup)
        {
            var allActiveSlugs = await _cache.GetOrCreateAsync(URLRECORD_ALL_ACTIVE_KEY,
                async () =>
                {
                    var allRecords = await _context
                        .UrlRecords.AsNoTracking()
                        .Where(x => x.IsActive)
                        .OrderByDescending(x => x.Id)
                        .ToListAsync();
                    var result = allRecords
                        .ToDictionary(x => GenerateKey(x.EntityId, x.EntityName),
                            x => x.Slug,
                            StringComparer.OrdinalIgnoreCase);
                    return result;
                },
                new CacheEntryOptions()
                {
                    AbsoluteExpiration = TimeSpan.FromHours(8)
                });
            var key = GenerateKey(entityId, entityName);
            if (!allActiveSlugs.TryGetValue(key, out slug))
            {
                return string.Empty;
            }
        }
        else
        {
            //TODO continue...
        }

        return slug;
    }

    protected bool TryGetPrefetchedActiveSlug(int entityId, string entityName, out string slug)
    {
        slug = null;
        
        if (_prefetchedUrlSlugCollections.TryGetValue(entityName, out var collection))
        {
            var urlRecord = collection.Find(entityId);
            if (urlRecord != null)
            {
                slug = urlRecord.Slug.NullIfEmpty();
            }
        }

        return slug != null;
    }

  

    private static string GenerateKey(int entityId, string entityName)
    {
        return entityId.ToString(CultureInfo.InvariantCulture) + entityName;
    }
}

public class UrlRecordCollection : IReadOnlyCollection<UrlRecord>
{
    private readonly string _entityName;
    private readonly IDictionary<string, UrlRecord> _dict;
    private readonly HashSet<int> _requestedSet;

    public UrlRecordCollection(string entityName, IEnumerable<int> requestedIds, IEnumerable<UrlRecord> urlRecords)
    {
        Guard.NotEmpty(entityName);
        Guard.NotNull(urlRecords);
        _entityName = entityName;
        _dict = urlRecords.ToDictionary(x => CreateKey(x.EntityId), x => x);
        if (requestedIds != null && requestedIds.Any())
        {
            _requestedSet = new HashSet<int>(requestedIds);
        }
    }

    public UrlRecord Find(int entityId)
    {
        if (_dict.TryGetValue(CreateKey(entityId), out var urlRecord))
        {
            return urlRecord;
        }

        if (_requestedSet != null && _requestedSet.Contains(entityId))
        {
            urlRecord = new UrlRecord()
            {
                EntityId = entityId,
                EntityName = _entityName,
            };
        }

        return urlRecord;
    }

    private static string CreateKey(int entityId)
        => entityId.ToString(CultureInfo.InvariantCulture);

    public IEnumerator<UrlRecord> GetEnumerator()
    {
        return _dict.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _dict.Values.GetEnumerator();
    }

    public int Count => _dict.Values.Count;
}