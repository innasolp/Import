using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Service;

public abstract class ImportService(ILogger logger, ILoaderService loaderService, string host)
    : IImportService
{
    public abstract string Name { get; }

    protected ILoaderService LoaderService { get; } = loaderService;

    protected ILogger Logger { get; } = logger;

    private bool? _isStarted = null;

    public bool IsStarted => _isStarted == true;

    protected object? _loadData;

    protected string Host { get; } = host;

    private readonly SemaphoreSlim _resetSemaphorSlim = new(1, 1);

    private bool _loaderIsReseted = false;   

    public virtual async Task Start(CancellationToken stoppingToken)
    {
        _loaderIsReseted = false;

        while (!stoppingToken.IsCancellationRequested)
        {
            await StartLoaderIfNeedAsync(stoppingToken);

            if (!LoaderService.IsStarted) break;
            else if (_isStarted == null)
            {
                _isStarted = true;
                _loadData = await LoaderService.GetData(Host, stoppingToken);
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
            catch (Exception)
            {
                _isStarted = false;
                throw;
            }
            finally
            {
                await Task.Delay(100);
            }
        }

        Logger.LogInformation(LogMessages.ServiceWasStopped, Name);
    }

    protected abstract Task ProcessAsync(CancellationToken stoppingToken);

    protected virtual async Task StartLoaderIfNeedAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && !LoaderService.IsStarted)
        {
            try
            {
                if (!LoaderService.IsStarted)
                    await LoaderService.Start(stoppingToken);
            }
            catch (ImportWarningException warning)
            {
                Logger.LogWarning(warning, LogMessages.LoaderNotStartedWarning, [LoaderService.Name, warning.Message]);
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogMessages.ImportWasStoppedWebLoaderNotExecute, LoaderService.Name);
                return;
            }
        }
    }

    protected virtual void HandleWarningException(ImportWarningException warning, string url)
    {
        Logger.LogWarning(warning, LogMessages.ProcessUrlNotCompleteWarning, [url, warning.Message]);
    }

    protected async Task ProcessUrlTaskAsync(Func<string, CancellationToken, Task> task, string url, CancellationToken cancellationToken = default)
    {
        try
        {
            await task(url, cancellationToken);
        }
        catch (LoaderServiceException loaderServiceEx)
        {
            await HandleLoaderServiceExceptionAsync(loaderServiceEx, url, cancellationToken);
        }
        catch (ImportWarningException warning)
        {
            HandleWarningException(warning, url);
        }
        catch (OperationCanceledException operationCancelledException)
        {
            HandleCancelling(operationCancelledException, url);
        }
        catch (Exception e)
        {
            HandleException(e, url);
        }
    }

    private async Task<ResultStatus> HandleLoaderServiceExceptionAsync(LoaderServiceException e, string url, CancellationToken cancellationToken = default)
    {
        if (e.NeedAction != null)
        {
            switch (e.NeedAction)
            {
                case LoaderServiceAction.Reset:
                    return await HandleResetingAsync(e, url, cancellationToken);

                case LoaderServiceAction.Wait:
                    Logger.LogWarning(e, LogMessages.RequestFailedAndLoaderWillBePaused, [url, e.Message, 500]);
                    await Task.Delay(500);
                    return await Task.FromResult(ResultStatus.Warning);

                default:
                    Logger.LogWarning(e, LogMessages.RequestUrlFailedWithError, [url, e.Message]);
                    return await Task.FromResult(ResultStatus.Warning);
            }
        }
        else
        {
            Logger.LogWarning(e.InnerException ?? e, LogMessages.RequestUrlFailedWithError, [url, e.Message]);
            return await Task.FromResult(ResultStatus.Warning);
        }
    }

    private async Task<ResultStatus> HandleResetingAsync(LoaderServiceException e, string url, CancellationToken cancellationToken = default)
    {
        if (_loaderIsReseted)
        {
            Logger.LogError(e, LogMessages.ImportWasStoppedLoaderServiceAlreadyReseted, [LoaderService.Name, Name]);
            var message = string.Format(Messages.ImportWasCancelledOnUrlBecauseLoaderFailed, [Name, url, LoaderService.Name]);
            throw new Exception(message, e);
        }

        Logger.LogWarning(e, LogMessages.LoadFromUrlCompletedWithErrorAndNeedReset, [url, e.Message]);

        try
        {
            await ResetLoaderAsync(url);

            Logger.LogInformation(LogMessages.LoaderResetSuccessfully);

            return await Task.FromResult(ResultStatus.Warning);
        }
        catch (Exception resetException)
        {
            Logger.LogError(resetException, LogMessages.LoaderServiceResetingFailed,
                [LoaderService.Name, resetException.Message]);

            return await Task.FromResult(ResultStatus.Error);
        }
        finally
        {
            await Task.Delay(100);
        }
    }

    private async Task ResetLoaderAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            await _resetSemaphorSlim.WaitAsync();

            Logger.LogInformation(LogMessages.LoaderIsReseting);
            await LoaderService.Reset(cancellationToken);
            await LoaderService.UpdateData(url, cancellationToken);
            _loadData = await LoaderService.GetData(Host, cancellationToken);
            _loaderIsReseted = true;
        }
        finally
        {
            _resetSemaphorSlim.Release();
        }
    }

    protected virtual void HandleException(Exception e, string url)
    {
        Logger.LogError(e, LogMessages.ProcessUrlFailedError, url);
    }

    protected virtual void HandleCancelling(OperationCanceledException operationCancelledException, string url)
    {
        if (operationCancelledException.InnerException != null)
        {
            Logger.LogError(operationCancelledException.InnerException, LogMessages.ServiceWasCancelledOnLoadingFromUrlByError,
                [Name, url, operationCancelledException.InnerException.Message]);            
        }
        else
        {
            Logger.LogInformation(LogMessages.ServiceWasCancelledOnLoadingFromUrl, Name, url);
            throw new OperationCanceledException(operationCancelledException.Message, operationCancelledException);
        }
    }

    protected virtual async Task HandleCancellingAsync(OperationCanceledException operationCancelledException)
    {
        if (operationCancelledException.InnerException != null)
        {
            Logger.LogError(operationCancelledException.InnerException, LogMessages.ServiceWasCancelledOn,
                [Name, operationCancelledException.InnerException.Message]);
            await Task.FromResult(false);
        }
        else
        {
            Logger.LogInformation(LogMessages.ServiceWasCancelled, Name);
            throw new OperationCanceledException(operationCancelledException.Message, operationCancelledException);
        }
    }

    protected async Task<UrlTaskResult<T>> ProcessUrlTaskAsync<T>(Func<string,CancellationToken, Task<T?>> task, string url, CancellationToken cancellationToken = default)
    {
        try
        {
            return UrlTaskResult<T>.Success(await task(url, cancellationToken), url);
        }
        catch (LoaderServiceException loaderServiceEx)
        {
            var resultStatus = await HandleLoaderServiceExceptionAsync(loaderServiceEx, url, cancellationToken);
            return await Task.FromResult(UrlTaskResult<T>.FromStatus(resultStatus, url, loaderServiceEx));
        }
        catch (ImportWarningException warning)
        {
            HandleWarningException(warning, url);
            return await Task.FromResult(UrlTaskResult<T>.Warning(default, url, warning));
        }
        catch (OperationCanceledException operationCancelledException)
        {
            HandleCancelling(operationCancelledException, url);
            return await Task.FromResult(UrlTaskResult<T>.Cancelled(url));
        }
        catch (Exception e)
        {
            HandleException(e, url);
            return await Task.FromResult(UrlTaskResult<T>.Failed(default, url, e));
        }
    }

    protected async Task<UrlTaskResult<T>> ProcessUrlTaskAsync<TUrl, T>(Func<TUrl, CancellationToken, Task<T?>> task, Func<TUrl, string> getUrl, TUrl itemUrl, CancellationToken cancellationToken = default)
    {
        var url = getUrl(itemUrl);
        try
        {
            return UrlTaskResult<T>.Success(await task(itemUrl, cancellationToken), url);
        }
        catch (LoaderServiceException loaderServiceEx)
        {
            var resultStatus = await HandleLoaderServiceExceptionAsync(loaderServiceEx, url, cancellationToken);
            return await Task.FromResult(UrlTaskResult<T>.FromStatus(resultStatus, url, loaderServiceEx));
        }
        catch (ImportWarningException warning)
        {
            HandleWarningException(warning, url);
            return await Task.FromResult(UrlTaskResult<T>.Warning(default, url, warning));
        }
        catch (OperationCanceledException operationCancelledException)
        {
            HandleCancelling(operationCancelledException, url);
            return await Task.FromResult(UrlTaskResult<T>.Cancelled(url));
        }
        catch (Exception e)
        {
            HandleException(e, url);
            return await Task.FromResult(UrlTaskResult<T>.Failed(default, url, e));
        }
    }

    protected async Task<TaskResult> ProcessTaskAsync(Func<Task> task)
    {
        try
        {
            await task();
            return TaskResult.Success();
        }
        catch (ImportWarningException warning)
        {
            return await Task.FromResult(TaskResult.Warning(warning));
        }
        catch (OperationCanceledException operationCancelledException)
        {
            await HandleCancellingAsync(operationCancelledException);
            return await Task.FromResult(TaskResult.Cancelled());
        }
        catch (Exception e)
        {
            return await Task.FromResult(TaskResult.Failed(e));
        }
    }

    protected async Task<TaskResult<T>> ProcessTaskAsync<T>(Func<Task<T?>> task)
    {
        try
        {
            return TaskResult<T>.Success(await task());
        }
        catch (ImportWarningException warning)
        {
            return await Task.FromResult(TaskResult<T>.Warning(default, warning));
        }
        catch (OperationCanceledException operationCancelledException)
        {
            await HandleCancellingAsync(operationCancelledException);
            return await Task.FromResult(TaskResult<T>.Cancelled());
        }
        catch (Exception e)
        {
            return await Task.FromResult(TaskResult<T>.Failed(default, e));
        }
    }

    protected virtual async Task<Stream> LoadFromUrlAsync(string url, CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => { if (LoaderService.IsStarted) Task.Run(()=>LoaderService.Close(cancellationToken)); });
        var stream = await LoaderService.Load(url, _loadData, cancellationToken);
        return await Task.FromResult(stream);
    }
}