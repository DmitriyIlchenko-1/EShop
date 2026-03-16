using System.Dynamic;
using EShop.Core.Data;
using EShop.Core.Platform.Caching;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Platform.Themes.Services;

public class DefaultThemeVariableService
{
    private const string ThemeVariables = "test";
    protected readonly ApplicationDbContext _db;
    protected readonly ICacheManager _cache;
    protected readonly IThemeRegistry _registry;

    public DefaultThemeVariableService(ICacheManager cache, ApplicationDbContext db, IThemeRegistry registry)
    {
        _cache = cache;
        _db = db;
        _registry = registry;
    }

    public virtual async Task<ExpandoObject> GetThemeVariablesAsync(string themeName)
    {
        if (themeName.IsEmpty() || !_registry.Contains(themeName))
            return null;

        var result = await _cache.GetOrCreateAsync(string.Format(ThemeVariables, themeName),
            async () =>
            {
                var dbVars = await _db
                    .ThemeVariables.AsNoTracking()
                    .Where(x => x.Theme == themeName)
                    .ToDictionaryAsync(x => x.Name, x => (object)x.Value);

                return MergeWithConfigVariables(_registry.GetThemeByName(themeName), dbVars);
            });

        return result;
    }

    /// <summary>
    /// <see href="https://stackoverflow.com/questions/31675973/unboxing-for-dynamic-type"/>
    /// </summary>
    private ExpandoObject MergeWithConfigVariables(ThemeDescriptor descriptor, Dictionary<string, object> dbVars)
    {
        Guard.NotNull(descriptor);
        var result = new ExpandoObject();
        var dict = (IDictionary<string, object>)result;
        descriptor.Variables.Values.Each(x =>  dict.Add(x.Name, x.DefaultValue));
        foreach (var varPair in dbVars)
        {
            if (varPair.Value is not null)
            {
                dict[varPair.Key] = varPair.Value;
            }
        }

        return result;
    }
}