namespace EShop.Infrastructure.Utilities;

public class Singleton<TService>
{
    private static TService _instance;

    public static TService? Instance
    {
        get => _instance;
        set => _instance = value;
    }
}