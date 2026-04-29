using EShop.Infrastructure.Extensions;
using EShop.Infrastructure.Utilities;

namespace EShop.Infrastructure.Domain;

public interface IMergedData
{
    public bool IgnoreMerge { get; set; }
    public Dictionary<string, object> MergedData { get; }
}

public static class MergedDataExtensions
{
    public static T GetMergedData<T>(this IMergedData obj, string propName, T defaultValue)
    {
        Guard.NotNull(obj);
        if (obj.IgnoreMerge)
            return defaultValue;
        if (obj.MergedData == null || obj.MergedData.Count == 0 || !propName.HasValue())
            return defaultValue;
        if (obj.MergedData.TryGetValue(propName, out var foundValue))
        {
            return (T)foundValue;
        }

        return defaultValue;
    }
    
    
}