using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Service;

public abstract class Service(ILogger logger) : IService, IAsyncDisposable
{
    private readonly Lock _ctsLock = new();
    private CancellationTokenSource? _stoppingCts;
    private Task? _executingTask;

    public abstract string Name { get; }
    protected ILogger Logger { get; } = logger;

    private int _connected;
    public bool Connected => Volatile.Read(ref _connected) == 1;

    private readonly SemaphoreSlim _startSemaphoreSlim = new(1, 1);
    private readonly SemaphoreSlim _connectedSemaphoreSlim = new(1, 1);

    public event AsyncEventHandler<ConnectedAsyncEventArgs>? ConnectedAsync;

    protected virtual async Task InvokeConnectedAsync(
        bool connected,
        Exception? exception = null,
        CancellationToken cancellationToken = default)
    {
        await _connectedSemaphoreSlim.WaitAsync(CancellationToken.None).ConfigureAwait(false);
        try
        {
            Volatile.Write(ref _connected, connected ? 1 : 0);

            var handlers = ConnectedAsync;
            if (handlers == null)
                return;

            var effectiveToken = cancellationToken.IsCancellationRequested
                ? CancellationToken.None
                : cancellationToken;

            var args = new ConnectedAsyncEventArgs(connected, exception, effectiveToken);

            var tasks = handlers.GetInvocationList()
                .OfType<AsyncEventHandler<ConnectedAsyncEventArgs>>()
                .Select(async handler =>
                {
                    try
                    {
                        await handler(this, args).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, LogMessages.ConnectedAsyncHandlerForServiceFailed, Name);
                    }
                });

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        finally
        {
            _connectedSemaphoreSlim.Release();
        }
    }

    public virtual async Task Start(CancellationToken cancellationToken)
    {
        await _startSemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            CancellationToken token;
            lock (_ctsLock)
            {
                _stoppingCts?.Dispose();
                _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                token = _stoppingCts.Token;
            }

            _executingTask = ExecuteAsync(token);
            await _executingTask.ConfigureAwait(false);
        }
        finally
        {
            _startSemaphoreSlim.Release();
        }
    }

    protected abstract Task<bool> ExecuteAsync(CancellationToken cancellationToken);

    public virtual async Task Stop(CancellationToken cancellationToken = default)
    {
        lock (_ctsLock)
        {
            _stoppingCts?.Cancel();
        }

        if (_executingTask != null)
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken))
                .ConfigureAwait(false);
        }
    }

    protected virtual Task CloseAsync() => Stop();

    protected virtual void Dispose()
    {
        lock (_ctsLock)
        {
            _stoppingCts?.Cancel();
            _stoppingCts?.Dispose();
            _stoppingCts = null;
        }

        _startSemaphoreSlim.Dispose();
        _connectedSemaphoreSlim.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync().ConfigureAwait(false);
        Dispose();
        GC.SuppressFinalize(this);
    }
}