using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Service;

public abstract class Service(ILogger logger) : IImportService, IAsyncDisposable
{
    private CancellationTokenSource? _stoppingCts;

    public abstract string Name { get; }

    protected ILogger Logger { get; } = logger;

    private event AsyncEventHandler<ConnectedAsyncEventArgs>? _connectedAsync;

    private readonly SemaphoreSlim _startSemaphoreSlim = new(1, 1);

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
        Interlocked.CompareExchange(ref _connectedAsync, _connectedAsync + value, _connectedAsync);
    }

    protected virtual void ConnectedRemove(AsyncEventHandler<ConnectedAsyncEventArgs> value)
    {
        Interlocked.CompareExchange(ref _connectedAsync, _connectedAsync - value, _connectedAsync);
    }

    protected async Task InvokeConnectedAsync(bool success, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        // Safe event invocation: capture and iterate to isolate handler failures
        var handlers = GetConnectedHandlers();
        if (handlers == null)
            return;

        var args = new ConnectedAsyncEventArgs(success, exception, cancellationToken);

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
            _startSemaphoreSlim.Release();
        }
    }

    protected abstract Task<bool> ExecuteAsync(CancellationToken cancellationToken);

    protected virtual void Dispose()
    {
        _startSemaphoreSlim.Dispose();

        _stoppingCts?.Cancel();
    }

    protected virtual Task CloseAsync()
    {
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