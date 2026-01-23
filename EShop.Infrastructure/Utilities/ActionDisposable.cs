using Microsoft.Win32.SafeHandles;

namespace EShop.Infrastructure.Utilities;

/// <summary>
/// Based on the anonymous disposal pattern.
/// </summary>
public struct ActionDisposable : IDisposable
{
    private Action _onDispose;
    public static readonly ActionDisposable Empty = new ActionDisposable();

    public ActionDisposable(Action onDispose)
    {
        ArgumentNullException.ThrowIfNull(onDispose);
        _onDispose = onDispose;
    }

    public void Dispose()
    {
 
        _onDispose?.Invoke();
        _onDispose = null; // Ensure it can't execute a second time.
 
    }
}