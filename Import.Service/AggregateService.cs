using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Service;


public class WorkItem(IImportService importService)
{
    public IImportService ImportService { get; } = importService;

    public bool Completed { get; internal protected set; }

    public bool? Success { get; internal protected set; }
}

public class AggregateService(ILogger logger, string serviceName, Func<IImportService, CancellationToken, Task> executionTask) 
    : Service(logger), IAggregateImportService
{
    private const int WorkitemsChunkSize = 10;

    private readonly Dictionary<Guid, WorkItem> _workItems = [];
    private readonly Dictionary<Guid, AsyncEventHandler<ConnectedAsyncEventArgs>> _workItemHandlers = [];

    private const int ServicesSemaphoreMaxCount = 1;
    private readonly SemaphoreSlim _servicesSemaphoreSlim = new(1,ServicesSemaphoreMaxCount);

    private readonly Func<IImportService, CancellationToken, Task> _executionTask = executionTask;

    public override string Name => serviceName;

    async Task IAggregateImportService.Enqueue(IImportService service, CancellationToken cancellationToken)
    {
        await _servicesSemaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            var guid = Guid.NewGuid();
            _workItems.Add(guid, new WorkItem(service));
            AsyncEventHandler<ConnectedAsyncEventArgs> serviceConnected = (sender, args) => ServiceItemConnectedAsync(guid, sender, args);
            service.ConnectedAsync += serviceConnected;
            _workItemHandlers.Add(guid, serviceConnected);
        }
        finally
        {
            ReleaseSemaphoreIfNeed();
        }
    }

    private async Task ServiceItemConnectedAsync(Guid guid, object sender, ConnectedAsyncEventArgs eventArgs)
    {
        if(sender is not IImportService service)
            throw new InvalidOperationException($"Invalid type {sender.GetType().Name}. Must be assignable to {nameof(IImportService)}.");

        if(!_workItems.TryGetValue(guid, out var workItem) || workItem == null)
            throw new InvalidOperationException($"No item with id {guid}.");

        if (eventArgs.Success)
        {
            workItem.Completed = false;
            workItem.Success = null;
            Logger.LogInformation(AggregateLogMessages.ServiceItemWasStarted, service.Name);
            return;
        }

        if (eventArgs.Exception == null)
        {
            workItem.Completed = true;
            workItem.Success = true;
            Logger.LogInformation(AggregateLogMessages.ServiceItemWasCompletedSuccessfully, service.Name);
        }
        else if (eventArgs.CancellationToken.IsCancellationRequested)
        {
            workItem.Completed = true;
            workItem.Success = false;
            Logger.LogInformation(AggregateLogMessages.ServiceItemWasCanceled, service.Name);
        }
        else
        {
            workItem.Completed = true;
            workItem.Success = false;
            Logger.LogInformation(AggregateLogMessages.ServiceItemHandlingWasFailedInfo, service.Name);
            Logger.LogError(eventArgs.Exception, AggregateLogMessages.ServiceItemWasFailedError, service.Name);
        }
    }

    IEnumerable<(IImportService Service, bool Completed, bool? Success)> IAggregateImportService.GetImportServices() 
        => [.. _workItems.Values.Select(w=>(w.ImportService, w.Completed, w.Success))];

    async Task<bool> IAggregateImportService.TryRemove(IImportService service, CancellationToken cancellationToken)
    {
        await _servicesSemaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            var workItem = _workItems.FirstOrDefault(w => w.Value.ImportService == service);
            if (workItem.Value == null) return false;
            RemoveConnectedHandlerIfAvailable(workItem);

            _workItems.Remove(workItem.Key);
            return true;
        }
        finally
        {
            ReleaseSemaphoreIfNeed();
        }
    }

    private void RemoveConnectedHandlerIfAvailable(KeyValuePair<Guid, WorkItem> workItem)
    {
        if (_workItemHandlers.TryGetValue(workItem.Key, out var serviceConnected))
        {
            workItem.Value.ImportService.ConnectedAsync -= serviceConnected;
            _workItemHandlers.Remove(workItem.Key);
        }
    }

    protected override async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            Logger.LogInformation(LogMessages.ServiceStarted, Name);

            await InvokeConnectedAsync(true, cancellationToken : cancellationToken);

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

    protected virtual async Task ProcessAsync(CancellationToken cancellationToken)
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            if(_workItems.Count == 0)
            {
                await Task.Delay(50, cancellationToken);
                continue;
            }

            var notCompletedWorkItems = new List<WorkItem>(_workItems.Values.Where(w=>!w.Completed));
            if (notCompletedWorkItems.Count == 0) continue;

            var workItemsChunk = notCompletedWorkItems.Take(WorkitemsChunkSize).ToList();
            var workItemsChunkTasks = workItemsChunk.Select(w=>ProcessWorkItem(w, cancellationToken));
            await Task.WhenAll(workItemsChunkTasks);
        }
    }

    private async Task ProcessWorkItem(WorkItem workItem, CancellationToken cancellationToken)
    {
        try
        {
            await _executionTask(workItem.ImportService, cancellationToken);            
        }
        catch(OperationCanceledException)
        {
            Logger.LogInformation(AggregateLogMessages.ServiceItemWasCanceled, workItem.ImportService.Name);
        }
        catch(Exception e)
        {
            Logger.LogInformation(AggregateLogMessages.ServiceItemHandlingWasFailedInfo, workItem.ImportService.Name);
            Logger.LogError(e, AggregateLogMessages.ServiceItemWasFailedError, workItem.ImportService.Name);
        }
    }

    private void ReleaseSemaphoreIfNeed()
    {
        if (_servicesSemaphoreSlim.CurrentCount < ServicesSemaphoreMaxCount)
            _servicesSemaphoreSlim.Release();
    }

    protected override Task CloseAsync()
    {
        ReleaseSemaphoreIfNeed();
        
        return base.CloseAsync();
    }

    protected override void Dispose()
    {
        foreach(var workItem in _workItems)
          RemoveConnectedHandlerIfAvailable(workItem);

        _servicesSemaphoreSlim.Dispose();

        base.Dispose();
    }
}