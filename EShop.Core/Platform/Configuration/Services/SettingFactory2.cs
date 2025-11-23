// using EShop.Core.Data;
// using EShop.Core.Platform.Configuration.Domain;
// using Microsoft.AspNetCore.Http;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Logging.Abstractions;
//
// namespace EShop.Core.Platform.Configuration.Services;
//
// public class SettingFactory2
// {
//     private readonly ApplicationDbContext _db;
//     private readonly IHttpContextAccessor _httpContextAccessor;
//     private readonly ILogger _logger = NullLogger.Instance;
//
//     public SettingFactory2(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
//     {
//         _db = db;
//         _httpContextAccessor = httpContextAccessor;
//     }
//
//     public async Task<T> LoadSettingsAsync<T>()
//     {
//         //TODO: add caching.
//
//         var rawSettings = await GetRawSettings(typeof(T));
//         return MaterializeSettings<T>(rawSettings);
//     }
//
//
//     private async Task<IDictionary<string, Setting>> GetRawSettings(Type settingsType)
//     {
//         ArgumentNullException.ThrowIfNull(settingsType);
//
//         string prefix = settingsType.Name + ".";
//         var settings = await _db
//             .Settings
//             .AsNoTracking()
//             .Where(x => x.Name.StartsWith(prefix))
//             .OrderBy(x => x.Name)
//             .ToListAsync();
//
//         return settings.ToDictionary(s => s.Name, s => s);
//     }
//
//     private T MaterializeSettings<T>(IDictionary<string, Setting> rawSettings)
//     {
//         var sType = typeof(T);
//         ArgumentNullException.ThrowIfNull(rawSettings);
//
//         var settingsInstance = Activator.CreateInstance<T>();
//         var prefix = sType.Name;
//
//         foreach (var prop in sType.GetProperties())
//         {
//             if (!prop.CanWrite)
//             {
//                 continue;
//             }
//
//             var key = prefix + "." + prop.Name;
//             rawSettings.TryGetValue(key, out var rawSetting);
//             var valueStr = rawSetting?.Value;
//             if (valueStr == null)
//             {
//                 continue;
//             }
//
//             try
//             {
//                 prop.SetValue(settingsInstance, Convert.ChangeType(valueStr, prop.PropertyType));
//             }
//             catch (Exception e)
//             {
//                 var msg = $"Couldn't convert setting {key} to type {prop.PropertyType}";
//                 throw;
//             }
//         }
//
//         return settingsInstance;
//     }
// }