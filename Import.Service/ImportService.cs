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

    private readonly SemaphoreSlim _loadSemaphoreSlim = new(1, 1);

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
            catch (Exception ex)
            {
                _isStarted = false;
                Logger.LogError(ex, LogMessages.ServiceFailedWithError, [Name, ex.Message]);
                break;
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
            catch (Exception e)
            {
                Logger.LogError(e, LogMessages.ImportWasStoppedWebLoaderNotExecute, LoaderService.Name);
                return;
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
                    Logger.LogWarning(e, LogMessages.RequestFailedAndLoaderWillBePaused, [url, e.Message, 500]);
                    await Task.Delay(500);
                    await Task.FromResult(ResultStatus.Warning);
                    break;

                default:
                    Logger.LogWarning(e, LogMessages.RequestUrlFailedWithError, [url, e.Message]);
                    break;
            }
        }
        else
        {
            Logger.LogWarning(e.InnerException ?? e, LogMessages.RequestUrlFailedWithError, [url, e.Message]);            
        }
    }

    private async Task HandleResetingAsync(LoaderServiceException e, string url, CancellationToken cancellationToken = default)
    {
        await _resetSemaphorSlim.WaitAsync(cancellationToken);

        if (_loaderIsReseted)
        {
            Logger.LogError(e, LogMessages.ImportWasStoppedLoaderServiceAlreadyReseted, [LoaderService.Name, Name]);
            var message = string.Format(Messages.ImportWasCancelledOnUrlBecauseLoaderFailed, [Name, url, LoaderService.Name]);
            _resetSemaphorSlim.Release();
            throw new ImportErrorException(message, e);
        }

        Logger.LogWarning(e, LogMessages.LoadFromUrlCompletedWithErrorAndNeedReset, [url, e.Message]);

        try
        {
            await ResetLoaderAsync(url, cancellationToken);

            Logger.LogInformation(LogMessages.LoaderResetSuccessfully);            
        }
        catch (Exception resetException)
        {
            Logger.LogError(resetException, LogMessages.LoaderServiceResetingFailed,
                [LoaderService.Name, resetException.Message]);
        }
        finally
        {
            _resetSemaphorSlim.Release();
        }
    }

    private async Task ResetLoaderAsync(string url, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation(LogMessages.LoaderIsReseting);
        await LoaderService.Reset(cancellationToken);
        await LoaderService.UpdateData(url, cancellationToken);
        _loadData = await LoaderService.GetData(Host, cancellationToken);
        _loaderIsReseted = true;
    }

    protected virtual async Task<(bool success, Stream? stream)> TryLoadFromUrlAsync(string url, CancellationToken cancellationToken)
    {
        await _loadSemaphoreSlim.WaitAsync(cancellationToken);

        cancellationToken.Register(() => { if (LoaderService.IsStarted) Task.Run(() => LoaderService.Close(cancellationToken), cancellationToken); });

        try
        {
            var stream = await LoaderService.Load(url, _loadData, cancellationToken);
            return (true, stream);
        }
        catch (LoaderServiceException loaderServiceException)
        {
            await HandleLoaderServiceExceptionAsync(loaderServiceException, url, cancellationToken);
            return (false, default(Stream));
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
}