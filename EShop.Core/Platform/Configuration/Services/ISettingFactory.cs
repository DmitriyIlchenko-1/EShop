using EShop.Core.Platform.Configuration.Domain;

namespace EShop.Core.Platform.Configuration.Services;

public interface ISettingFactory
{
    Task<T> LoadSettingsAsync<T>() where T : ISettings, new();
    Task<ISettings> LoadSettingsAsync(Type settingsType);
}