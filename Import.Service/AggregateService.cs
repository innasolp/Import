using Import.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

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

    private readonly ConcurrentDictionary<Guid, WorkItem> _workItems = [];
    private readonly ConcurrentDictionary<Guid, AsyncEventHandler<ConnectedAsyncEventArgs>> _workItemHandlers = [];

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
            _workItems.TryAdd(guid, new WorkItem(service));
            AsyncEventHandler<ConnectedAsyncEventArgs> serviceConnected = (sender, args) => ServiceItemConnectedAsync(guid, sender, args);
            service.ConnectedAsync += serviceConnected;
            _workItemHandlers.TryAdd(guid, serviceConnected);
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

        await _servicesSemaphoreSlim.WaitAsync();

        try
        {
            if (!_workItems.TryGetValue(guid, out var workItem) || workItem == null)
                throw new InvalidOperationException($"No item with id {guid}.");

            if (eventArgs.Connected)
            {
                workItem.Completed = false;
                workItem.Success = null;
                Logger.LogInformation(AggregateLogMessages.ServiceItemWasStarted, service.Name);
                return;
            }

            workItem.Completed = true;

            if (eventArgs.Exception == null)
            {
                workItem.Success = true;
                Logger.LogInformation(AggregateLogMessages.ServiceItemWasCompletedSuccessfully, service.Name);
            }
            else if (eventArgs.CancellationToken.IsCancellationRequested)
            {
                workItem.Success = false;
                Logger.LogInformation(AggregateLogMessages.ServiceItemWasCanceled, service.Name);
            }
            else
            {
                workItem.Success = false;
                Logger.LogInformation(AggregateLogMessages.ServiceItemHandlingWasFailedInfo, service.Name);
                Logger.LogError(eventArgs.Exception, AggregateLogMessages.ServiceItemWasFailedError, service.Name);
            }
        }
        finally
        {
            ReleaseSemaphoreIfNeed();
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

            _workItems.TryRemove(workItem.Key, out var _);
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
            _workItemHandlers.TryRemove(workItem.Key, out var _);
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
            if(_workItems.IsEmpty)
            {
                await Task.Delay(50, cancellationToken);
                continue;
            }

            var workItems = new List<WorkItem>(_workItems.Values);
            var notCompletedWorkItems = workItems.Where(w=>!w.Completed).ToList();
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