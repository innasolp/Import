using Import.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Import.Service;


public class WorkItem(IService importService)
{
    private int _completed;
    private int _successState;

    public IService ImportService { get; } = importService;

    public bool Completed
    {
        get => Volatile.Read(ref _completed) == 1;
        protected internal set => Volatile.Write(ref _completed, value ? 1 : 0);
    }

    public bool? Success
    {
        get => Volatile.Read(ref _successState) switch
        {
            1 => false,
            2 => true,
            _ => null
        };
        protected internal set => Volatile.Write(ref _successState, value switch
        {
            false => 1,
            true => 2,
            null => 0
        });
    }

    internal AsyncEventHandler<ConnectedAsyncEventArgs>? ConnectedHandler { get; set; }
}

public class AggregateService(
    ILogger logger,
    string serviceName,
    Func<IService, CancellationToken, Task> executionTask)
    : Service(logger), IAggregateImportService
{
    private const int WorkitemsChunkSize = 10;

    private readonly ConcurrentDictionary<IService, WorkItem> _workItems = new(ReferenceEqualityComparer.Instance);
    private readonly Func<IService, CancellationToken, Task> _executionTask = executionTask;

    public override string Name => serviceName;

    Task IAggregateImportService.Enqueue(IService service, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var workItem = new WorkItem(service);
        AsyncEventHandler<ConnectedAsyncEventArgs> handler = (sender, args) =>
            ServiceItemConnectedAsync(workItem, sender, args);

        workItem.ConnectedHandler = handler;

        service.ConnectedAsync += handler;

        if (!_workItems.TryAdd(service, workItem))
        {
            service.ConnectedAsync -= handler;
        }

        return Task.CompletedTask;
    }

    private Task ServiceItemConnectedAsync(WorkItem workItem, object sender, ConnectedAsyncEventArgs eventArgs)
    {
        if (sender is not IService service)
            throw new InvalidOperationException($"Invalid type {sender.GetType().Name}. Must be assignable to {nameof(IService)}.");

        if (eventArgs.Connected)
        {
            workItem.Completed = false;
            workItem.Success = null;
            Logger.LogInformation(AggregateLogMessages.ServiceItemWasStarted, service.Name);
            return Task.CompletedTask;
        }

        if (eventArgs.CancellationToken.IsCancellationRequested || eventArgs.Exception is OperationCanceledException)
        {
            workItem.Success = false;
            Logger.LogInformation(AggregateLogMessages.ServiceItemWasCanceled, service.Name);
        }
        else if (eventArgs.Exception != null)
        {
            workItem.Success = false;
            Logger.LogInformation(AggregateLogMessages.ServiceItemHandlingWasFailedInfo, service.Name);
            Logger.LogError(eventArgs.Exception, AggregateLogMessages.ServiceItemWasFailedError, service.Name);
        }
        else
        {
            workItem.Success = true;
            Logger.LogInformation(AggregateLogMessages.ServiceItemWasCompletedSuccessfully, service.Name);
        }

        workItem.Completed = true;
        return Task.CompletedTask;
    }

    IEnumerable<(IService Service, bool Completed, bool? Success)> IAggregateImportService.GetImportServices()
        => [.. _workItems.Values.Select(w => (w.ImportService, w.Completed, w.Success))];

    Task<bool> IAggregateImportService.TryRemove(IService service, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_workItems.TryRemove(service, out var workItem))
            return Task.FromResult(false);

        DetachHandler(workItem);
        return Task.FromResult(true);
    }

    private static void DetachHandler(WorkItem workItem)
    {
        if (workItem.ConnectedHandler is { } handler)
        {
            workItem.ImportService.ConnectedAsync -= handler;
            workItem.ConnectedHandler = null;
        }
    }

    protected override async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            Logger.LogInformation(LogMessages.ServiceStarted, Name);

            await InvokeConnectedAsync(true, cancellationToken: cancellationToken).ConfigureAwait(false);
            await ProcessAsync(cancellationToken).ConfigureAwait(false);
            await InvokeConnectedAsync(false, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException e)
        {
            Logger.LogWarning(e, LogMessages.ServiceWasCancelled, Name);
            await InvokeConnectedAsync(false, e, cancellationToken).ConfigureAwait(false);
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, LogMessages.ServiceFailedWithError, Name);
            await InvokeConnectedAsync(false, ex, cancellationToken).ConfigureAwait(false);
            return false;
        }

        Logger.LogInformation(LogMessages.ServiceWasStopped, Name);
        return true;
    }

    protected virtual async Task ProcessAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {

            var workItemsChunk = _workItems.Values
                .Where(w => !w.Completed)
                .Take(WorkitemsChunkSize)
                .ToList();

            if (workItemsChunk.Count == 0)
            {
                await Task.Delay(50, cancellationToken).ConfigureAwait(false);
                continue;
            }

            var workItemsChunkTasks = workItemsChunk.Select(w => ProcessWorkItem(w, cancellationToken));
            await Task.WhenAll(workItemsChunkTasks).ConfigureAwait(false);
        }
    }

    private async Task ProcessWorkItem(WorkItem workItem, CancellationToken cancellationToken)
    {
        try
        {
            await _executionTask(workItem.ImportService, cancellationToken).ConfigureAwait(false);

            if (!workItem.Completed)
            {
                workItem.Success ??= true;
                workItem.Completed = true;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            workItem.Success = false;
            workItem.Completed = true;
            Logger.LogInformation(AggregateLogMessages.ServiceItemWasCanceled, workItem.ImportService.Name);
            throw;
        }
        catch (OperationCanceledException)
        {
            workItem.Success = false;
            workItem.Completed = true;
            Logger.LogInformation(AggregateLogMessages.ServiceItemWasCanceled, workItem.ImportService.Name);
        }
        catch (Exception e)
        {
            workItem.Success = false;
            workItem.Completed = true;
            Logger.LogInformation(AggregateLogMessages.ServiceItemHandlingWasFailedInfo, workItem.ImportService.Name);
            Logger.LogError(e, AggregateLogMessages.ServiceItemWasFailedError, workItem.ImportService.Name);
        }
    }

    protected override void Dispose()
    {
        foreach (var workItem in _workItems.Values)
            DetachHandler(workItem);

        _workItems.Clear();
        base.Dispose();
    }
}