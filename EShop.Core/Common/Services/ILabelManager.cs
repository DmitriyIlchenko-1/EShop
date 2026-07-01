using System.Xml;
using System.Xml.Linq;
using EShop.Core.Common.Domain;
using EShop.Core.Data;
using EShop.Core.Platform.Caching;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Data;
using EShop.Infrastructure.Engine;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace EShop.Core.Common.Services;

public interface ILabelManager
{
    Task<string> GetLabelIconAsync(string iconName, IDictionary<string, object> parameters = null);
    // Task<Label> GetLabelByNameAsync(string name, bool tracking = false);
}

public class DefaultLabelManager : ILabelManager
{
    private readonly ICacheManager _cache;
    private readonly IFileProvider _fileProvider;
    private const string IconPath = "/icons/";
    const string IconFileFormat = "{0}-icon.svg";
    private const string LabelIconHashCodeKey = "labelicon:{0}";
    private readonly ApplicationDbContext _db;

    public DefaultLabelManager(ICacheManagerFactory f, IApplicationContext app, ApplicationDbContext db)
    {
        _cache = f.GetMemoryCache();
        _fileProvider = app.WebRoot;
        _db = db;
    }

    public async Task<string> GetLabelIconAsync(string labelName, IDictionary<string, object> parameters = null)
    {
        parameters ??= new Dictionary<string, object>();
        var cacheKey = string.Format(LabelIconHashCodeKey, GenerateIconHash(labelName, parameters));
        return await _cache.GetOrCreateAsync(cacheKey,
            async () =>
            {
                string iconFileName = string.Format(IconFileFormat,
                    labelName
                        .Trim()
                        .Replace(" ", "-", StringComparison.OrdinalIgnoreCase));
                var iconInfo = _fileProvider.GetFileInfo(IconPath + iconFileName);
                await using var stream = iconInfo.CreateReadStream();
                var xmlDocument = await XElement.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
                foreach (var parameter in parameters)
                {
                    xmlDocument.SetAttributeValue(parameter.Key, parameter.Value);
                }
                var reader = xmlDocument.CreateReader();
                reader.MoveToContent();
                return reader.ReadOuterXml();
            },
            new CacheEntryOptions() { AbsoluteExpiration = TimeSpan.FromSeconds(60) });
    }

    private string GenerateIconHash(string name, IDictionary<string, object> parameters)
    {
        var hash = HashCodeCombiner.Start();
        hash.Add(name);
        foreach (var p in parameters)
        {
            hash.Add(p.Key);
            hash.Add(p.Value);
        }

        return hash.GetCombinedHash64();
    }
}