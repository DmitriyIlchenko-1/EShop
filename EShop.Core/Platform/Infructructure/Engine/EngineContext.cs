using EShop.Infrastructure.Utilities;

namespace EShop.Infrastructure.Engine;

public class EngineContext
{
    public static IEngine Create()
    {
        return Singleton<IEngine>.Instance ??= new EShopEngine();
    }

    public static IEngine Current
        => Create();
}