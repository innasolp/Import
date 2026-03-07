using System.Resources;
using Import.Service.Test.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Import.Service.Test;

public class ServiceTest
{
    [Fact]
    public async Task Start_ShouldInvokeExecuteEachTime()
    {
        var loggerMock = new Mock<ILogger>();
        await using var service = new TestService(loggerMock.Object, $"{Guid.NewGuid()}");

        await service.Start(CancellationToken.None);
        await service.Start(CancellationToken.None);

        Assert.Equal(2, service.ExecuteCallCount);
    }

    [Fact]
    public async Task Stop_ShouldCancelExecutionToken()
    {
        var loggerMock = new Mock<ILogger>();
        await using var service = new TestService(loggerMock.Object, $"{Guid.NewGuid()}");

        var startedTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancelledTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        service.ExecuteAsyncImpl = async token =>
        {
            startedTcs.TrySetResult();
            try
            {
                await Task.Delay(Timeout.Infinite, token);
                return true;
            }
            catch (OperationCanceledException)
            {
                cancelledTcs.TrySetResult();
                return true;
            }
        };

        var startTask = service.Start(CancellationToken.None);
        await startedTcs.Task;

        await service.Stop();

        await Task.WhenAny(cancelledTcs.Task, Task.Delay(TimeSpan.FromSeconds(2)));

        Assert.True(cancelledTcs.Task.IsCompleted);
        await startTask;
    }

    [Fact]
    public async Task InvokeConnectedAsync_ShouldLogAndContinueWhenHandlerFails()
    {
        var loggerMock = new Mock<ILogger>();
        await using var service = new TestService(loggerMock.Object, $"{Guid.NewGuid()}");

        var handlerCalledTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        service.ConnectedAsync += async (_, _) =>
        {
            handlerCalledTcs.TrySetResult();
            await Task.CompletedTask;
        };

        var exception = new InvalidOperationException("handler failed");
        service.ConnectedAsync += (_, _) => throw exception;

        await service.RaiseConnectedAsync(true);

        await handlerCalledTcs.Task;
        var logResourceManager = new ResourceManager("Import.Service.LogMessages", typeof(Service).Assembly);
        var messageFormat = logResourceManager.GetString("ConnectedAsyncHandlerForServiceFailed")
            ?? throw new InvalidOperationException("Log message ConnectedAsyncHandlerForServiceFailed not found");

        loggerMock.VerifyError(exception, messageFormat, service.Name);
    }

    private sealed class TestService : Service
    {
        public TestService(ILogger logger, string name) : base(logger)
        {
            Name = name;
        }

        public override string Name { get; }

        public int ExecuteCallCount { get; private set; }

        public Func<CancellationToken, Task<bool>>? ExecuteAsyncImpl { get; set; }

        protected override Task<bool> ExecuteAsync(CancellationToken cancellationToken)
        {
            ExecuteCallCount++;
            if (ExecuteAsyncImpl != null)
                return ExecuteAsyncImpl(cancellationToken);

            return Task.FromResult(true);
        }

        public Task RaiseConnectedAsync(bool success, Exception? exception = null, CancellationToken cancellationToken = default)
        {
            return InvokeConnectedAsync(success, exception, cancellationToken);
        }
    }
}
