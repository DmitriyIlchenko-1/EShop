namespace EShop.Infrastructure.Utilities;

public struct ActionDisposable : IDisposable
{
    private readonly Action _execute;

    public ActionDisposable(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _execute = action;
    }

    public void Dispose()
    {
        _execute();
    }
}