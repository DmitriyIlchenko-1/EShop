using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace EShop.Infrastructure.Utilities;

public static class StringBuilderPool
{
    public static ObjectPool<StringBuilder> Pool { get; }
        = new DefaultObjectPoolProvider().CreateStringBuilderPool();
}


 