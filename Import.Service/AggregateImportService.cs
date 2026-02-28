using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Service;

public class AggregateImportService(ILogger logger, string serviceName, Func<IEnumerable<IImportService>, CancellationToken, Task> executionTask) : Service(logger), IAggregateImportService
{
    private readonly List<IImportService> _importServices = [];

    private readonly SemaphoreSlim _servicesSemaphoreSlim = new(1);

    private readonly Func<IEnumerable<IImportService>, CancellationToken, Task> _executionTask = executionTask;

    public override string Name => serviceName;

    async Task IAggregateImportService.Enqueue(IImportService service, CancellationToken cancellationToken)
    {
        await _servicesSemaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            _importServices.Add(service);
        }
        finally
        {
            _servicesSemaphoreSlim.Release();
        }
    }

    IEnumerable<IImportService> IAggregateImportService.GetImportServices() => _importServices;

    async Task IAggregateImportService.Dequeue(IImportService service, CancellationToken cancellationToken)
    {
        await _servicesSemaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            _importServices.Remove(service);
        }
        catch
        {
            _servicesSemaphoreSlim.Release();
        }
    }

    protected override async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {            
            await InvokeConnectedAsync(true, cancellationToken : cancellationToken);

            var importServices = new List<IImportService>(_importServices);

            await _executionTask(importServices, cancellationToken);

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
}