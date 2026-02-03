using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Service;

public abstract class ImportService(ILogger logger, ILoaderService loaderService, string host)
    : IImportService, IAsyncDisposable
{
    private CancellationTokenSource? _stoppingCts;

    public abstract string Name { get; }

    public event AsyncEventHandler<ConnectedAsyncEventArgs> ConnectedAsync;

    protected ILoaderService LoaderService { get; } = loaderService;

    protected ILogger Logger { get; } = logger;

    private object? LoaderData { get; set; }

    protected string Host { get; } = host;

    private readonly SemaphoreSlim _loadSemaphoreSlim = new(1, 1);

    private readonly SemaphoreSlim _startSemaphoreSlim = new(1, 1);

    private int _resetTryCount = 0;

    private const int MaxResetTryCount = 3;

    private const int WaitingLoaderDelayMilliseconds = 500;

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

    private async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            await StartLoaderIfNeededAsync(cancellationToken);

            if (!LoaderService.IsStarted)
            {
                await InvokeConnectedAsync(false, cancellationToken: cancellationToken);
                return false;
            }
            else
            {
                try
                {
                    await LoadData(cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, LogMessages.ServiceFailedWithError, Name);
                    await InvokeConnectedAsync(false, ex, cancellationToken);
                    return false;
                }

                await InvokeConnectedAsync(true, cancellationToken: cancellationToken);

                Logger.LogInformation(LogMessages.ServiceStarted, Name);
            }

            await ProcessAsync(cancellationToken);

            await InvokeConnectedAsync(false, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException e)
        {
            Logger.LogWarning(e, LogMessages.ServiceWasCancelled, Name);
            await InvokeConnectedAsync(false, e, cancellationToken);
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, LogMessages.ServiceFailedWithError, Name);
            await InvokeConnectedAsync(false, ex, cancellationToken);
            return false;
        }

        Logger.LogInformation(LogMessages.ServiceWasStopped, Name);
        return true;
    }

    private async Task CloseLoaderAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            await LoaderService.Close(cts.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, LogMessages.FailedToCloseLoaderForService, Name);
        }
    }

    private async Task LoadData(CancellationToken cancellationToken)
    {
        LoaderData = await LoaderService.GetData(Host, cancellationToken).ConfigureAwait(false);
        Logger.LogInformation(LogMessages.LoaderDataReceivedSuccessfully, LoaderService.Name);
    }

    protected object? GetLoaderData() => LoaderData;

    private async Task InvokeConnectedAsync(bool success, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        // Safe event invocation: capture and iterate to isolate handler failures
        var handlers = ConnectedAsync;
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

    protected abstract Task ProcessAsync(CancellationToken cancellationToken);

    protected virtual async Task StartLoaderIfNeededAsync(CancellationToken cancellationToken)
    {
        const int backoffMs = 500;

        int tryCount = 0;
        int maxTryCount = 5;

        while (!LoaderService.IsStarted)
        {
            try
            {
                await LoaderService.Start(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                tryCount++;
                Logger.LogError(e, LogMessages.FailedToStartLoader, LoaderService.Name);

                if (tryCount >= maxTryCount)
                {
                    throw;
                }

                try
                {
                    await Task.Delay(backoffMs, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
            }
        }
    }

    private async Task HandleLoaderServiceExceptionAsync(LoaderServiceException e, string url, CancellationToken cancellationToken = default)
    {
        if (e.NeedAction != null)
        {
            switch (e.NeedAction)
            {
                case LoaderServiceAction.Reset:
                    await HandleResettingAsync(e, url, cancellationToken);
                    break;

                case LoaderServiceAction.Wait:
                    Logger.LogWarning(e, LogMessages.RequestFailedAndLoaderWillBePaused, url, e.Message, WaitingLoaderDelayMilliseconds);
                    await Task.Delay(WaitingLoaderDelayMilliseconds, cancellationToken);
                    break;

                default:
                    Logger.LogWarning(e, LogMessages.RequestUrlFailedWithError, url, e.Message);
                    break;
            }
        }
        else
        {
            Logger.LogWarning(e.InnerException ?? e, LogMessages.RequestUrlFailedWithError, url, e.Message);
        }
    }

    private async Task HandleResettingAsync(LoaderServiceException e, string url, CancellationToken cancellationToken = default)
    {
        //await _resetSemaphoreSlim.WaitAsync(cancellationToken);

        if (_resetTryCount >= MaxResetTryCount)
        {
            Logger.LogError(e, LogMessages.ImportWasStoppedLoaderServiceAlreadyReseted, LoaderService.Name, Name);
            var message = string.Format(Messages.ImportWasCancelledOnUrlBecauseLoaderFailed, new object[] { Name, url, LoaderService.Name });
            //_resetSemaphoreSlim.Release();
            throw new ImportErrorException(message, e);
        }

        Logger.LogWarning(e, LogMessages.LoadFromUrlCompletedWithErrorAndNeedReset, url, e.Message);

        try
        {
            await ResetLoaderAsync(url, cancellationToken);

            Logger.LogInformation(LogMessages.LoaderResetSuccessfully);
        }
        catch (Exception resetException)
        {
            Logger.LogError(resetException, LogMessages.LoaderServiceResettingFailed, LoaderService.Name, resetException.Message);
        }
    }

    private async Task ResetLoaderAsync(string url, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(LogMessages.LoaderIsResetting, _resetTryCount + 1);

        try
        {
            await LoaderService.Reset(cancellationToken).ConfigureAwait(false);
            await LoaderService.UpdateData(url, cancellationToken).ConfigureAwait(false);
            await LoadData(cancellationToken);
        }
        finally
        {
            Interlocked.Increment(ref _resetTryCount);
        }
    }

    protected virtual async Task<(bool success, Stream? stream)> TryLoadFromUrlAsync(string url, object? data, CancellationToken cancellationToken = default)
    {
        await _loadSemaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var stream = await LoaderService.Load(url, data, cancellationToken).ConfigureAwait(false);
            Interlocked.Exchange(ref _resetTryCount, 0);
            return (true, stream);
        }
        catch (LoaderServiceException loaderServiceException)
        {
            await HandleLoaderServiceExceptionAsync(loaderServiceException, url, cancellationToken);
            return (false, default(Stream));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new LoadFromUrlException(url, e.Message, e);
        }
        finally
        {
            _loadSemaphoreSlim.Release();
        }
    }

    protected virtual void Dispose()
    {
        _startSemaphoreSlim.Dispose();
        _loadSemaphoreSlim.Dispose();

        _stoppingCts?.Cancel();
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await Stop();

        if (LoaderService.IsStarted)
            await CloseLoaderAsync();

        Dispose();

        _stoppingCts?.Dispose();
    }

    public virtual Task Stop(CancellationToken cancellationToken = default)
    {
        _stoppingCts?.Cancel();
        return Task.CompletedTask;
    }
}