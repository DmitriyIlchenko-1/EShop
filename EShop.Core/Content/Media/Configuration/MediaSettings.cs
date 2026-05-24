using EShop.Core.Platform.Configuration.Domain;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Core.Content.Media.Configuration;

public class MediaSettings : ISettings
{
    public int MaxImageWidth { get; set; }  
    public int MaxHeight { get; set; } 
    public int HttpCacheDuration { get; set; }

    public ResponseCacheLocation CacheType 
    {
        get => (ResponseCacheLocation)_cacheType;
        set => _cacheType = (int)value;
    }
    private int _cacheType = 2;


}