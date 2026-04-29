using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using EShop.Core.Data;
using EShop.Core.Platform.Caching;
using EShop.Core.Platform.Themes.Domain;
using EShop.Infrastructure.Caching;
using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Core.Platform.Themes.Services;

public interface IThemeVariableService
{
    Task<ExpandoObject> GetThemeVariablesAsync(string themeName);
    Task<string> GenerateCssVarFile(string themeName);
}

public class DefaultThemeVariableService : IThemeVariableService
{
    private const string ThemeVariables = "web:theme-variables-{0}";
    private const string VarFormat = "--{0}: {1};";
    private readonly ApplicationDbContext _db;
    private readonly ICacheManager _cache;
    private readonly IThemeRegistry _registry;

    public DefaultThemeVariableService(ICacheManagerFactory cacheFactory, ApplicationDbContext db, IThemeRegistry registry)
    {
        _cache = cacheFactory.GetMemoryCache();
        _db = db;
        _registry = registry;
    }

    public virtual async Task<ExpandoObject> GetThemeVariablesAsync(string themeName)
    {
        if (themeName.IsEmpty() || !_registry.Contains(themeName))
            return null;

        string cacheKey = string.Format(CultureInfo.InvariantCulture, ThemeVariables, themeName);
        var result = await _cache.GetOrCreateAsync(cacheKey,
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
    
    public virtual async Task<string> GenerateCssVarFile(string themeName)
    {
        Guard.NotEmpty(themeName);
        var variables = await GetThemeVariablesAsync(themeName) as IDictionary<string, object>;
        if (variables.Count == 0)
            return string.Empty;
        using var _ = StringBuilderPool.Pool.Get(out var builder);
        foreach (var variable in variables)
        {
            builder.AppendFormat(VarFormat, variable.Key, variable.Value);
        }

        return builder.ToString();
    }

     


    /// <summary>
    /// <see href="https://stackoverflow.com/questions/31675973/unboxing-for-dynamic-type"/>
    /// </summary>
    private ExpandoObject MergeWithConfigVariables(ThemeDescriptor descriptor, Dictionary<string, object> dbVars)
    {
        Guard.NotNull(descriptor);
        var result = new ExpandoObject();
        var dict = (IDictionary<string, object>)result;
        descriptor.Variables.Values.Each(x => dict.Add(x.Name, x.DefaultValue));
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