using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Service;


public class WorkItem(IImportService importService)
{
    public IImportService ImportService { get; } = importService;

    public bool Completed { get; internal protected set; }

    public bool? Success { get; internal protected set; }
}

public class AggregateImportService(ILogger logger, string serviceName, Func<IImportService, CancellationToken, Task> executionTask) 
    : Service(logger), IAggregateImportService
{
    private const int WorkitemsChunkSize = 10;

    private readonly List<WorkItem> _workItems = [];

    private readonly SemaphoreSlim _servicesSemaphoreSlim = new(1);

    private readonly Func<IImportService, CancellationToken, Task> _executionTask = executionTask;

    public override string Name => serviceName;

    async Task IAggregateImportService.Enqueue(IImportService service, CancellationToken cancellationToken)
    {
        await _servicesSemaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            _workItems.Add(new WorkItem(service));
        }
        finally
        {
            _servicesSemaphoreSlim.Release();
        }
    }

    IEnumerable<(IImportService Service, bool Completed, bool? Success)> IAggregateImportService.GetImportServices() 
        => [.. _workItems.Select(w=>(w.ImportService, w.Completed, w.Success))];

    async Task<bool> IAggregateImportService.TryRemove(IImportService service, CancellationToken cancellationToken)
    {
        await _servicesSemaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            var workItem = _workItems.FirstOrDefault(w => w.ImportService == service);
            if (workItem == null) return false;

            _workItems.Remove(workItem);
            return true;
        }
        finally
        {
            _servicesSemaphoreSlim.Release();
        }
    }

    protected override async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {            
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

            var notCompletedWorkItems = new List<WorkItem>(_workItems.Where(w=>!w.Completed));
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
            workItem.Completed = true;
            workItem.Success = true;
            Logger.LogInformation("Service item {ServiceName} was completed successfully", workItem.ImportService.Name);
        }
        catch(OperationCanceledException)
        {
            workItem.Completed = true;
            workItem.Success = false;
            Logger.LogInformation("Service item {ServiceName} was canceled", workItem.ImportService.Name);
        }
        catch(Exception e)
        {
            workItem.Completed = true;
            workItem.Success = false;
            Logger.LogInformation("Service item {ServiceName} handling was failed, see error log.", workItem.ImportService.Name);
            Logger.LogError(e, "Service item {ServiceName} was failed with error.", workItem.ImportService.Name);
        }
    }

    protected override async Task CloseAsync()
    {
        _servicesSemaphoreSlim.Release();
        _servicesSemaphoreSlim.Dispose();

        await base.CloseAsync();
    }
}