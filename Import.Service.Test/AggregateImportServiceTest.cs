using Import.Interfaces;
using Import.Service.Test.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Import.Service.Test;

public class AggregateImportServiceTest
{
    [Fact]
    public async Task Enqueue_ShouldAddPendingService()
    {
        var loggerMock = new Mock<ILogger>();
        await using var service = CreateService(loggerMock, (_, _) => Task.CompletedTask);
        var aggregate = (IAggregateImportService)service;

        var importService = new Mock<IImportService>();
        importService.SetupGet(s => s.Name).Returns("service-1");

        await aggregate.Enqueue(importService.Object);

        var items = aggregate.GetImportServices().ToList();

        Assert.Single(items);
        Assert.Equal(importService.Object, items[0].Service);
        Assert.False(items[0].Completed);
        Assert.Null(items[0].Success);
    }

    [Fact]
    public async Task TryRemove_ShouldRemoveExistingService()
    {
        var loggerMock = new Mock<ILogger>();
        await using var service = CreateService(loggerMock, (_, _) => Task.CompletedTask);
        var aggregate = (IAggregateImportService)service;

        var importService = new Mock<IImportService>();
        importService.SetupGet(s => s.Name).Returns("service-1");

        await aggregate.Enqueue(importService.Object);

        var removed = await aggregate.TryRemove(importService.Object);

        Assert.True(removed);
        Assert.Empty(aggregate.GetImportServices());

        var removedAgain = await aggregate.TryRemove(importService.Object);

        Assert.False(removedAgain);
    }

    [Fact]
    public async Task Start_ShouldProcessWorkItemSuccessfully()
    {
        var loggerMock = new Mock<ILogger>();
        var processed = new TaskCompletionSource<bool>();
        await using var service = CreateService(loggerMock, (_, _) =>
        {
            processed.TrySetResult(true);
            return Task.CompletedTask;
        });
        var aggregate = (IAggregateImportService)service;

        var importService = new Mock<IImportService>();
        importService.SetupGet(s => s.Name).Returns("service-1");
        await aggregate.Enqueue(importService.Object);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var startTask = service.Start(cts.Token);

        await processed.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await WaitForConditionAsync(() => aggregate.GetImportServices().All(w => w.Completed), TimeSpan.FromSeconds(1));

        var items = aggregate.GetImportServices().ToList();
        Assert.True(items[0].Completed);
        Assert.True(items[0].Success);

        loggerMock.VerifyInfo("Service item {ServiceName} was completed successfully", importService.Object.Name);

        cts.Cancel();
        await startTask.WaitAsync(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Start_ShouldMarkWorkItemCancelledWhenExecutionCancelled()
    {
        var loggerMock = new Mock<ILogger>();
        var processed = new TaskCompletionSource<bool>();
        await using var service = CreateService(loggerMock, (_, _) =>
        {
            processed.TrySetResult(true);
            throw new OperationCanceledException();
        });
        var aggregate = (IAggregateImportService)service;

        var importService = new Mock<IImportService>();
        importService.SetupGet(s => s.Name).Returns("service-cancelled");
        await aggregate.Enqueue(importService.Object);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var startTask = service.Start(cts.Token);

        await processed.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await WaitForConditionAsync(() => aggregate.GetImportServices().All(w => w.Completed), TimeSpan.FromSeconds(1));

        var items = aggregate.GetImportServices().ToList();
        Assert.True(items[0].Completed);
        Assert.False(items[0].Success);

        loggerMock.VerifyInfo("Service item {ServiceName} was canceled", importService.Object.Name);

        cts.Cancel();
        await startTask.WaitAsync(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Start_ShouldMarkWorkItemFailedWhenExecutionThrows()
    {
        var loggerMock = new Mock<ILogger>();
        var processed = new TaskCompletionSource<bool>();
        var failure = new InvalidOperationException("failure");
        await using var service = CreateService(loggerMock, (_, _) =>
        {
            processed.TrySetResult(true);
            throw failure;
        });
        var aggregate = (IAggregateImportService)service;

        var importService = new Mock<IImportService>();
        importService.SetupGet(s => s.Name).Returns("service-failed");
        await aggregate.Enqueue(importService.Object);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var startTask = service.Start(cts.Token);

        await processed.Task.WaitAsync(TimeSpan.FromSeconds(1));
        await WaitForConditionAsync(() => aggregate.GetImportServices().All(w => w.Completed), TimeSpan.FromSeconds(1));

        var items = aggregate.GetImportServices().ToList();
        Assert.True(items[0].Completed);
        Assert.False(items[0].Success);

        loggerMock.VerifyInfo("Service item {ServiceName} handling was failed, see error log.", importService.Object.Name);
        loggerMock.VerifyError(failure, "Service item {ServiceName} was failed with error.", importService.Object.Name);

        cts.Cancel();
        await startTask.WaitAsync(TimeSpan.FromSeconds(1));
    }

    private static AggregateImportService CreateService(Mock<ILogger> loggerMock, Func<IImportService, CancellationToken, Task> executionTask)
    {
        return new AggregateImportService(loggerMock.Object, "aggregate-service", executionTask);
    }

    private static async Task WaitForConditionAsync(Func<bool> condition, TimeSpan timeout)
    {
        var start = DateTime.UtcNow;
        while (!condition())
        {
            if (DateTime.UtcNow - start > timeout)
                throw new TimeoutException("Condition was not satisfied within the allotted time.");

            await Task.Delay(10);
        }
    }
}
