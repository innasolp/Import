using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Service;

public abstract class ImportService(ILogger logger, ILoaderService loaderService, string host)
    : IImportService
{
    public abstract string Name { get; }

    public event AsyncEventHandler<ConnectedAsyncEventArgs> ConnectedAsync;

    protected ILoaderService LoaderService { get; } = loaderService;

    protected ILogger Logger { get; } = logger;

    private bool? _isStarted = null;

    public bool IsStarted => _isStarted == true;

    protected object? LoadData { get; private set; }

    protected string Host { get; } = host;

    private readonly SemaphoreSlim _resetSemaphoreSlim = new(1, 1);

    private readonly SemaphoreSlim _loadSemaphoreSlim = new(1, 1);

    private int _resetTryCount = 0;

    private const int _maxResetTryCount = 3;

    public virtual async Task Start(object? parameter, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await StartLoaderIfNeedAsync(stoppingToken);

            if (!LoaderService.IsStarted)
            {
                await InvokeConnectedAsync(false, parameter, stoppingToken);
                break;
            }
            else if (_isStarted == null)
            {
                try
                {
                    LoadData = await LoaderService.GetData(Host, stoppingToken);
                    _isStarted = true;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _isStarted = false;
                    Logger.LogError(ex, LogMessages.ServiceFailedWithError, [Name, ex.Message]);
                    await InvokeConnectedAsync(false, parameter, stoppingToken);
                    break;
                }

                await InvokeConnectedAsync(true, parameter, stoppingToken);

                Logger.LogInformation(LogMessages.ServiceStarted, Name);
            }

            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _isStarted = false;
                Logger.LogError(ex, LogMessages.ServiceFailedWithError, Name, ex.Message);
                break;
            }
            finally
            {
                if (LoaderService.IsStarted)
                {
                    try
                    {
                        await LoaderService.Close(cancellationToken: stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, LogMessages.FailedToCloseLoaderForService, Name);
                    }
                }

                await Task.Delay(100);                
            }
        }

        Logger.LogInformation(LogMessages.ServiceWasStopped, Name);
    }

    private async Task InvokeConnectedAsync(bool success, object? parameter, CancellationToken cancellationToken)
    {
        // Safe event invocation: capture and iterate to isolate handler failures
        var handlers = ConnectedAsync;
        if (handlers == null)
            return;

        var args = new ConnectedAsyncEventArgs(success, parameter, null, cancellationToken);

        foreach (var d in handlers.GetInvocationList())
        {
            if (d is AsyncEventHandler<ConnectedAsyncEventArgs> handler)
            {
                try
                {
                    var t = handler.Invoke(this, args);
                    if (t != null)
                        await t;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, LogMessages.ConnectedAsyncHandlerForServiceFailed, Name);
                    // continue to next handler
                }
            }
        }
    }

    protected abstract Task ProcessAsync(CancellationToken stoppingToken);

    protected virtual async Task StartLoaderIfNeedAsync(CancellationToken stoppingToken)
    {
       var backoffMs = 200;
        const int maxBackoffMs = 5000;

        while (!stoppingToken.IsCancellationRequested && !LoaderService.IsStarted)
        {
            try
            {
                if (!LoaderService.IsStarted)
                    await LoaderService.Start(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogMessages.ImportWasStoppedWebLoaderNotExecute ?? "Failed to start loader {Loader}", LoaderService.Name);
                try
                {
                    await Task.Delay(backoffMs, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }

                backoffMs = Math.Min(backoffMs * 2, maxBackoffMs);
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
                    await HandleResetingAsync(e, url, cancellationToken);
                    break;

                case LoaderServiceAction.Wait:
                    Logger.LogWarning(e, LogMessages.RequestFailedAndLoaderWillBePaused, url, e.Message, 500);
                    await Task.Delay(500, cancellationToken);
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

    private async Task HandleResetingAsync(LoaderServiceException e, string url, CancellationToken cancellationToken = default)
    {
        await _resetSemaphoreSlim.WaitAsync(cancellationToken);

        if (_resetTryCount >= _maxResetTryCount)
        {
            Logger.LogError(e, LogMessages.ImportWasStoppedLoaderServiceAlreadyReseted, LoaderService.Name, Name);
            var message = string.Format(Messages.ImportWasCancelledOnUrlBecauseLoaderFailed, new object[] { Name, url, LoaderService.Name });
            _resetSemaphoreSlim.Release();
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
            Logger.LogError(resetException, LogMessages.LoaderServiceResetingFailed, LoaderService.Name, resetException.Message);
        }
        finally
        {
            _resetSemaphoreSlim.Release();
        }
    }

    private async Task ResetLoaderAsync(string url, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(LogMessages.LoaderIsReseting);

        await LoaderService.Reset(cancellationToken);
        await LoaderService.UpdateData(url, cancellationToken);
        LoadData = await LoaderService.GetData(Host, cancellationToken);
        _resetTryCount++;
    }

    protected virtual async Task<(bool success, Stream? stream)> TryLoadFromUrlAsync(string url, object? data, CancellationToken cancellationToken = default)
    {
        bool released = false;
        await _loadSemaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            try
            {
                var stream = await LoaderService.Load(url, data, cancellationToken);
                return (true, stream);
            }
            catch (LoaderServiceException loaderServiceException)
            {
                _loadSemaphoreSlim.Release();
                released = true;

                await HandleLoaderServiceExceptionAsync(loaderServiceException, url, cancellationToken);
                return (false, default(Stream));
            }
            catch (Exception e)
            {
                throw new LoadFromUrlException(url, e.Message, e);
            }
        }
        finally
        {
            if (!released)
                _loadSemaphoreSlim.Release();
        }
    }
}