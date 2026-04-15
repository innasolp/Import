using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Service;

public abstract class Service(ILogger logger) : IImportService, IAsyncDisposable
{
    private CancellationTokenSource? _stoppingCts;

    public abstract string Name { get; }

    protected ILogger Logger { get; } = logger;

    private int _connected; // 0 = false, 1 = true

    public bool Connected => Volatile.Read(ref _connected) == 1;

    private event AsyncEventHandler<ConnectedAsyncEventArgs>? _connectedAsync;

    private const int SemaphoreMaxCount = 1; 

    private readonly SemaphoreSlim _startSemaphoreSlim = new(1, SemaphoreMaxCount);

    private readonly SemaphoreSlim _connectedSemaphoreSlim = new(1, SemaphoreMaxCount);

    public event AsyncEventHandler<ConnectedAsyncEventArgs> ConnectedAsync
    {
        add
        {
            ConnectedAdd(value);
        }
        remove
        {
            ConnectedRemove(value);
        }
    }

    protected virtual void ConnectedAdd(AsyncEventHandler<ConnectedAsyncEventArgs> value)
    {
        _connectedAsync += value;
    }

    protected virtual void ConnectedRemove(AsyncEventHandler<ConnectedAsyncEventArgs> value)
    {
        _connectedAsync -= value;
    }

    protected virtual async Task InvokeConnectedAsync(bool connected, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        await SetConnectedAsync(connected);

        var handlers = GetConnectedHandlers();
        if (handlers == null)
            return;

        var args = new ConnectedAsyncEventArgs(connected, exception, cancellationToken);

        async Task HandlerTask(AsyncEventHandler<ConnectedAsyncEventArgs> handler)
        {
            try
            {
                await handler(this, args).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogMessages.ConnectedAsyncHandlerForServiceFailed, Name);
            }
        };

        var tasks = handlers.GetInvocationList()
            .OfType<AsyncEventHandler<ConnectedAsyncEventArgs>>()
            .Select(HandlerTask);

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    protected async Task SetConnectedAsync(bool connected)
    {
        await _connectedSemaphoreSlim.WaitAsync();

        try
        {
            Interlocked.Exchange(ref _connected, connected ? 1 : 0);
        }
        finally
        {
            ReleaseSemaphoreIfNeed(_connectedSemaphoreSlim);
        }
    }

    protected AsyncEventHandler<ConnectedAsyncEventArgs>? GetConnectedHandlers()
    {
        var handlers = _connectedAsync;
        return handlers;
    }

    public virtual async Task Start(CancellationToken cancellationToken)
    {
        await _startSemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _stoppingCts?.Dispose();
            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await ExecuteAsync(_stoppingCts.Token);
        }
        finally
        {
            ReleaseSemaphoreIfNeed(_startSemaphoreSlim);
        }
    }

    protected abstract Task<bool> ExecuteAsync(CancellationToken cancellationToken);

    protected virtual void Dispose()
    {
        _startSemaphoreSlim.Dispose();

        _stoppingCts?.Cancel();
    }

    private static void ReleaseSemaphoreIfNeed(SemaphoreSlim semaphoreSlim)
    {
        if (semaphoreSlim.CurrentCount < SemaphoreMaxCount)
            semaphoreSlim.Release();
    }

    protected virtual Task CloseAsync()
    {
        ReleaseSemaphoreIfNeed(_startSemaphoreSlim);
        ReleaseSemaphoreIfNeed(_connectedSemaphoreSlim);

        return Stop();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await CloseAsync();

        Dispose();

        _stoppingCts?.Dispose();
    }

    public virtual Task Stop(CancellationToken cancellationToken = default)
    {
        _stoppingCts?.Cancel();
        return Task.CompletedTask;
    }
}