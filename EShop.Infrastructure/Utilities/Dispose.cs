using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EShop.Infrastructure.Utilities;

/// <summary>
/// All non-sealed classes that implement IDisposable should be considered a potential base class, because they can be inherited.
/// If you implement the dispose pattern for any potential base class, you should inherit this type to help you
/// provide the needed code to make sure unmanaged resources and memory are cleaned up properly,
/// in case a subclass doesn't wrap unmanaged resources in a SafeHandler.
/// </summary>
/// <remarks>
/// 1. https://stackoverflow.com/a/46692381/
/// 2. https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose#implement-the-dispose-pattern-for-a-derived-class
/// </remarks>
public class Disposable : IDisposable, IAsyncDisposable
{
    private const int DisposedFlag = 1;
    private int _isDisposed;

    public void Dispose()
    {
        // assign a 1 to the _isDisposed and returns its previous value
        if (Interlocked.Exchange(ref _isDisposed, DisposedFlag) == DisposedFlag)
            return;

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _isDisposed, DisposedFlag) == DisposedFlag)
            return default;

        GC.SuppressFinalize(this);
        return DisposeAsync(true);
    }

    protected virtual ValueTask DisposeAsync(bool disposing)
    {
        Dispose(disposing);
        return default;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    protected internal bool IsDisposed
    {
        get
        {
            Interlocked.MemoryBarrier();
            return _isDisposed == DisposedFlag;
        }
    }
    
    protected internal void CheckDisposed()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(this.GetType().ShortDisplayName());
    }
    
    
}