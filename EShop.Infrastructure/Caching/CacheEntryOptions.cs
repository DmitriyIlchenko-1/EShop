using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace EShop.Infrastructure.Caching;

 
public class CacheEntryOptions
{
    private TimeSpan _absoluteExpiration;
    private TimeSpan? _slidingExpiration;
    public TimeSpan AbsoluteExpiration
    {
        get => _absoluteExpiration;
        set
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(AbsoluteExpiration),
                    value,
                    "The absolute expiration value must be positive.");
            }

            _absoluteExpiration = value;
        }
    }
    
    public TimeSpan? SlidingExpiration
    {
        get => _slidingExpiration;
        set
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(SlidingExpiration),
                    value,
                    "The sliding expiration value must be positive.");
            }
            _slidingExpiration = value;
        }
    }
}

 