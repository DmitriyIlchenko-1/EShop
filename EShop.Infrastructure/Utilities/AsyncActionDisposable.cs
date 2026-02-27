namespace EShop.Infrastructure.Utilities;

public struct AsyncActionDisposable : IAsyncDisposable
{
    private Func<ValueTask> _onDispose;
    public static readonly AsyncActionDisposable Empty = new AsyncActionDisposable();

    public AsyncActionDisposable(Func<ValueTask> onDispose)
    {
        ArgumentNullException.ThrowIfNull(onDispose);
        _onDispose = onDispose;
    }

    public async ValueTask DisposeAsync()
    {
        if (_onDispose == null)
            return;
        
        await _onDispose().ConfigureAwait(false);
        _onDispose = null; // Ensure it can't execute a second time.
    }
}