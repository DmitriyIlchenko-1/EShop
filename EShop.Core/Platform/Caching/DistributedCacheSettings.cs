 
using EShop.Core.Platform.Configuration.Domain;

namespace EShop.Core.Platform.Caching;

public class DistributedCacheSettings : ISettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}