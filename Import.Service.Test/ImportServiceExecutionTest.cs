using Import.Interfaces;
using Import.Service.Test.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Import.Service.Test;

public abstract class ImportServiceExecutionTest<TService, TLogger>(ITestOutputHelper outputHelper) 
    : ImportServiceTest<TService, TLogger>(outputHelper)
    where TService : ImportService
    where TLogger : class, ILogger
{
    protected const int timeout = 1000;

    protected const int longTimeout = timeout * 3;

    protected const int resetTimeout = 15000;

    protected async Task ShouldLogImportWasStoppedWhenLoaderNotExecutedAsync()
    {
        var messageResourceKey = "FailedToStartLoader";
        var resultMessageFormat = LogResourceManager.GetString(messageResourceKey)
            ?? throw new InvalidOperationException($"message format {messageResourceKey} is null or not found");

        var infoMessageResourceKey = "ServiceWasStopped";
        var infoMessageFormat = LogResourceManager.GetString(infoMessageResourceKey)
            ?? throw new InvalidOperationException($"message format {infoMessageResourceKey} is null or not found");


        var exception = new InvalidOperationException("test fatal error");
        var name = $"{Guid.NewGuid()}";
        var service = CreateService(name);

        LoaderMock.Setup(l => l.Name).Returns($"{Guid.NewGuid()}");
        LoaderMock.Setup(w => w.Start(It.IsAny<CancellationToken>())).ThrowsAsync(exception);

        using var tokenSource = new CancellationTokenSource();
        
        await Task.WhenAny(service.Start(tokenSource.Token), Task.Delay(longTimeout));        

        LoggerMock.VerifyInfo(infoMessageFormat, name);

        LoggerMock.VerifyError(exception, resultMessageFormat, LoaderMock.Object.Name);
    }

    protected async Task ShouldLogServiceStartedWhenLoaderExecutesSuccessfullyAsync()
    {
        var messageResourceKey = "ServiceStarted";
        var resultMessageFormat = LogResourceManager.GetString(messageResourceKey)
            ?? throw new InvalidOperationException($"message format {messageResourceKey} is null or not found");

        var name = $"{Guid.NewGuid()}";
        var service = CreateService(name);

        LoaderMock.SetupStartSuccess();

        using var tokenSource = new CancellationTokenSource();
        
        await Task.WhenAny(service.Start(tokenSource.Token), Task.Delay(timeout));

        LoaderMock.Verify(l => l.Start(It.IsAny<CancellationToken>()), Times.Once);

        LoggerMock.VerifyInfo(resultMessageFormat, name);
    }

    protected async Task ShouldLogImportStoppedWhenCancellationRequestedAsync()
    {
        var messageResourceKey = "ServiceWasStopped";
        var resultMessageFormat = LogResourceManager.GetString(messageResourceKey)
            ?? throw new InvalidOperationException($"message format {messageResourceKey} is null or not found");

        var cancelledMessageResourceKey = "ServiceWasCancelled";
        var cancelledMessageFormat = LogResourceManager.GetString(cancelledMessageResourceKey)
            ?? throw new InvalidOperationException($"message format {cancelledMessageResourceKey} is null or not found");

        var name = $"{Guid.NewGuid()}";
        var service = CreateService(name);

        LoaderMock.SetupStartSuccess();

        using var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(TimeSpan.FromMilliseconds(timeout));

        await service.Start(tokenSource.Token);

        LoggerMock.VerifyWarning(cancelledMessageFormat, name);
        LoggerMock.VerifyInfo(resultMessageFormat, name);
    }

    protected async Task ShouldLogResettingErrorIfLoaderServiceNeedsResettingAsync()        
    {
        var loaderResetDelay = 500;

        var messageResourceKey = "ImportWasStoppedLoaderServiceAlreadyReseted";
        var resultMessageFormat = LogResourceManager.GetString(messageResourceKey) 
            ?? throw new InvalidOperationException($"message format {messageResourceKey} is null or not found");

        var service = CreateService($"{Guid.NewGuid()}");

        LoaderMock.Setup(l => l.Name).Returns($"{Guid.NewGuid()}");
        LoaderMock.SetupStartSuccess();

        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);

        var loaderServiceException = new LoaderServiceException("loading failed.", LoaderServiceAction.Reset);
        LoaderMock.Setup(l => l.Load(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>())).ThrowsAsync(loaderServiceException);

        using var tokenSource = new CancellationTokenSource();
        
        await Task.WhenAny(service.Start(tokenSource.Token), Task.Delay(resetTimeout));

        LoaderMock.Verify(l => l.Start(It.IsAny<CancellationToken>()), Times.Once);

        LoggerMock.VerifyError(loaderServiceException, resultMessageFormat, LoaderMock.Object.Name, service.Name);
    }


    protected async Task ShouldLogServiceFailedErrorWhenUnhandledExceptionThrownAsync()
    {
        var messageResourceKey = "ServiceFailedWithError";
        var resultMessageFormat = LogResourceManager.GetString(messageResourceKey)
            ?? throw new InvalidOperationException($"message format {messageResourceKey} is null or not found");

        var service = CreateService($"{Guid.NewGuid()}");

        LoaderMock.Setup(l => l.Name).Returns($"{Guid.NewGuid()}");
        LoaderMock.SetupStartSuccess();

        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);

        var exception = new Exception("loading failed.");
        LoaderMock.Setup(l => l.Load(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);

        using var tokenSource = new CancellationTokenSource();
        
        await Task.WhenAny(service.Start(tokenSource.Token), Task.Delay(timeout));

        LoaderMock.Verify(l => l.Start(It.IsAny<CancellationToken>()), Times.Once);

        LoggerMock.VerifyError(exception, (v,e)=>v.InnerException == e, resultMessageFormat, service.Name, exception.Message);
    }

    protected async Task ShouldLogRequestFailedAndLoaderWillBePausedWarningWhenLoaderNeedsWaitAsync()
    {
        var messageResourceKey = "RequestFailedAndLoaderWillBePaused";
        var resultMessageFormat = LogResourceManager.GetString(messageResourceKey)
            ?? throw new InvalidOperationException($"message format {messageResourceKey} is null or not found");

        var innerException = new Exception("status code 403");
        var exception = new LoaderServiceException("request forbidden", innerException, LoaderServiceAction.Wait);

        LoaderMock.Setup(l => l.Name).Returns($"{Guid.NewGuid()}");
        LoaderMock.SetupStartSuccess();
        LoaderMock.Setup(w => w.Load(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);

        var service = CreateService($"{Guid.NewGuid()}");

        using var tokenSource = new CancellationTokenSource();

        await Task.WhenAny(service.Start(tokenSource.Token), Task.Delay(timeout));

        LoaderMock.Verify(l => l.Start(It.IsAny<CancellationToken>()), Times.Once);

        LoggerMock.VerifyWarning(exception, resultMessageFormat);
    }
}