using Import.Interfaces;
using Import.Interfaces.Exceptions;
using Microsoft.Extensions.Logging;

namespace Import.Service;

public abstract class ImportService(ILogger logger, ILoaderService loaderService, string host)
    : Service(logger)
{  
    protected ILoaderService LoaderService { get; } = loaderService;

    private object? LoaderData { get; set; }

    protected string Host { get; } = host;

    private const int LoaderMaxCount = 1;
    private readonly SemaphoreSlim _loadSemaphoreSlim = new(1, LoaderMaxCount);

    private int _resetTryCount = 0;

    private const int MaxResetTryCount = 3;

    private int _retryCount = 0;

    private const int MaxRetryCount = 3;

    protected override async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
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

    private async Task<bool> HandleLoaderServiceExceptionForContinueAsync(LoaderServiceException e, string url, CancellationToken cancellationToken = default)
    {
        if (e.NeedsAction != null)
        {
            switch (e.NeedsAction)
            {
                case LoaderServiceAction.Reset:
                    await HandleResettingAsync(e, url, cancellationToken);
                    return true;

                case LoaderServiceAction.RetryAfter:
                    return await HandleRetryAsync(e, url, cancellationToken);

                default:
                    Logger.LogWarning(e, LogMessages.RequestUrlFailedWithError, url, e.Message);
                    break;
            }
        }
        else
        {
            Logger.LogWarning(e.InnerException ?? e, LogMessages.RequestUrlFailedWithError, url, e.Message);
        }

        return false;
    }

    private async Task<bool> HandleRetryAsync(LoaderServiceException e, string url, CancellationToken cancellationToken)
    {
        if (e is not RetryAfterLoaderServiceException retryAfterEx)
            throw new InvalidOperationException($"Invalid loader error {e.GetType().Name} for action {LoaderServiceAction.RetryAfter}.");

        if (_retryCount >= MaxRetryCount)
        {
            Interlocked.Exchange(ref _retryCount, 0);
            return false;
        }

        Logger.LogWarning(e, LogMessages.RequestFailedAndLoaderWillBePaused, url, e.Message, retryAfterEx.RetryAfter.TotalMilliseconds);
        await Task.Delay(retryAfterEx.RetryAfter, cancellationToken);
        Interlocked.Increment(ref _retryCount);
        return true;
    }

    private async Task HandleResettingAsync(LoaderServiceException e, string url, CancellationToken cancellationToken = default)
    {
        if (_resetTryCount >= MaxResetTryCount)
        {
            Logger.LogError(e, LogMessages.ImportWasStoppedLoaderServiceAlreadyReseted, LoaderService.Name, Name);
            var message = string.Format(Messages.ImportWasCancelledOnUrlBecauseLoaderFailed, new object[] { Name, url, LoaderService.Name });
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

        var loaded = false;

        try
        {
            while (!loaded && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var stream = await LoaderService.Load(url, data, cancellationToken).ConfigureAwait(false);
                    loaded = true;
                    Interlocked.Exchange(ref _resetTryCount, 0);
                    Interlocked.Exchange(ref _retryCount, 0);
                    return (true, stream);
                }
                catch (LoaderServiceException loaderServiceException)
                {
                    if(!await HandleLoaderServiceExceptionForContinueAsync(loaderServiceException, url, cancellationToken))
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
            }

            return (false, null);
        }
        finally
        {
            _loadSemaphoreSlim.Release();
        }
    }

    protected override async Task CloseAsync()
    {
        if (_loadSemaphoreSlim.CurrentCount < LoaderMaxCount)
        {
            _loadSemaphoreSlim.Release();
        }       

        await  base.CloseAsync();

        if (LoaderService.IsStarted)
            await CloseLoaderAsync();
    }

    protected override void Dispose()
    {
        _loadSemaphoreSlim.Dispose();

        base.Dispose();
    }
}