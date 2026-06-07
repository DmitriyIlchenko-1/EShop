using System.Diagnostics.CodeAnalysis;
using EShop.Infrastructure.Utilities;
using Microsoft.AspNetCore.Http;

namespace EShop.Infrastructure.Extensions;

public static class HttpContextExtensions
{
    public static TValue GetItem<TValue>(this HttpContext context, string key,
        Func<TValue> dataRetriever = null)
    {
        Guard.NotNull(context);
        Guard.NotEmpty(key);
        if (context.Items.TryGetValue(key, out var result))
        {
            return (TValue)result;
        }

        var factoryResult = dataRetriever != null ? dataRetriever() : default(TValue);
        context.Items[key] = factoryResult;
        return factoryResult;
    }
    
    public static async Task<TValue> GetItemAsync<TValue>(this HttpContext context, string key,
        Func<Task<TValue>> dataRetriever = null)
    {
        Guard.NotNull(context);
        Guard.NotEmpty(key);
        if (context.Items.TryGetValue(key, out var result))
        {
            return (TValue)result;
        }

        var factoryResult = dataRetriever != null ? await dataRetriever() : default(TValue);
        context.Items[key] = factoryResult;
        return factoryResult;
    }

     
}