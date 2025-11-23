using System.IO.Hashing;
using System.Text;

namespace EShop.Infrastructure.Utilities;

public class HashCodeCombiner
{
    private long _combinedHash64;

    public static HashCodeCombiner Start()
        => new HashCodeCombiner();

    public int GetCombinedHash()
    {
        return _combinedHash64.GetHashCode();
    }

    public HashCodeCombiner Add<T>(T value, IEqualityComparer<T>? comparer = null)
    {
        if (value is string str)
        {
            Append((long)XxHash3.HashToUInt64(Encoding.UTF8.GetBytes(str)));
        }
        else if (value is not null)
        {
            Append(comparer?.GetHashCode(value) ?? value.GetHashCode());
        }
        
        return this;
    }

    private void Append(long hash)
    {
        if (hash != 0)
        {
            _combinedHash64 = ((_combinedHash64 << 5) + _combinedHash64) ^ hash;
        }
    }
}