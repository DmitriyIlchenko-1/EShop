using System.ComponentModel;
using EShop.Core.Data;
using EShop.Core.Platform.Configuration.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EShop.Core.Platform.Configuration.Services;

public class SettingFactory : ISettingFactory
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger = NullLogger.Instance;

    public SettingFactory(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<T> LoadSettingsAsync<T>() where T : ISettings, new()
    {
        return (T)await LoadSettingsAsync(typeof(T));
    }

    public async Task<ISettings> LoadSettingsAsync(Type settingsType)
    {
        ArgumentNullException.ThrowIfNull(settingsType);

        if (!typeof(ISettings).IsAssignableFrom(settingsType))
        {
            throw new ArgumentException($"The type provided doesn't implement {typeof(ISettings).FullName}.",
                nameof(settingsType));
        }

        //TODO: add caching.

        var rawSettings = await GetRawSettings(settingsType);
        return MaterializeSettings(settingsType, rawSettings);
    }

    private async Task<IDictionary<string, Setting>> GetRawSettings(Type settingsType)
    {
        ArgumentNullException.ThrowIfNull(settingsType);

        string prefix = settingsType.Name + ".";
        var settings = await _db
            .Settings
            .AsNoTracking()
            .Where(x => x.Name.StartsWith(prefix))
            .OrderBy(x => x.Name)
            .ToListAsync();

        return settings.ToDictionary(s => s.Name, s => s);
    }

    private ISettings MaterializeSettings(Type settingsType, IDictionary<string, Setting> rawSettings)
    {
        ArgumentNullException.ThrowIfNull(settingsType);
        ArgumentNullException.ThrowIfNull(rawSettings);

        var settingsInstance = (ISettings)Activator.CreateInstance(settingsType);
        var prefix = settingsType.Name;

        foreach (var prop in settingsType.GetProperties())
        {
            if (!prop.CanWrite)
            {
                continue;
            }

            var key = prefix + "." + prop.Name;
            rawSettings.TryGetValue(key, out var rawSetting);
            var valueStr = rawSetting?.Value;
            if (valueStr == null)
            {
                continue;
            }

            var descriptor = TypeDescriptor.GetConverter(prop.PropertyType);
            if (!descriptor.CanConvertFrom(typeof(string)))
                continue;
            if (!descriptor.IsValid(valueStr))
                continue;

            var value = descriptor.ConvertFromInvariantString(valueStr);
            prop.SetValue(settingsInstance, value);
        }

        return settingsInstance;
    }
}