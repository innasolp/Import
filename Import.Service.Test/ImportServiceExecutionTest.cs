using Import.Interfaces.Exceptions;
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
    protected async Task ShouldLogImportWasStoppedWithErrorWhenLoaderNotExecutedAsync(int cancellationTimeoutInMilliseconds)
    {
        var messageResourceKey = "FailedToStartLoader";
        var resultMessageFormat = LogResourceManager.GetString(messageResourceKey)
            ?? throw new InvalidOperationException($"message format {messageResourceKey} is null or not found");

        var serviceErrorMessageResourceKey = "ServiceFailedWithError";
        var serviceErrorMessageFormat = LogResourceManager.GetString(serviceErrorMessageResourceKey)
            ?? throw new InvalidOperationException($"message format {serviceErrorMessageResourceKey} is null or not found");


        var exception = new InvalidOperationException("test fatal error");
        var name = $"{Guid.NewGuid()}";
        var service = CreateService(name);

        LoaderMock.Setup(l => l.Name).Returns($"{Guid.NewGuid()}");
        LoaderMock.Setup(w => w.Start(It.IsAny<CancellationToken>())).ThrowsAsync(exception);

        using var tokenSource = new CancellationTokenSource();
        
        await Task.WhenAny(service.Start(tokenSource.Token), Task.Delay(cancellationTimeoutInMilliseconds));

        LoggerMock.VerifyError(exception, resultMessageFormat, LoaderMock.Object.Name); 
        
        LoggerMock.VerifyError(exception, serviceErrorMessageFormat, name);       
    }

    protected async Task ShouldLogServiceStartedWhenLoaderExecutesSuccessfullyAsync(int cancellationTimeoutMilliseconds)
    {
        var messageResourceKey = "ServiceStarted";
        var resultMessageFormat = LogResourceManager.GetString(messageResourceKey)
            ?? throw new InvalidOperationException($"message format {messageResourceKey} is null or not found");

        var name = $"{Guid.NewGuid()}";
        var service = CreateService(name);

        LoaderMock.Setup(l => l.Name).Returns($"{Guid.NewGuid()}");
        LoaderMock.SetupStartSuccess();

        using var tokenSource = new CancellationTokenSource();
        
        await Task.WhenAny(service.Start(tokenSource.Token), Task.Delay(cancellationTimeoutMilliseconds));

        LoaderMock.Verify(l => l.Start(It.IsAny<CancellationToken>()), Times.Once);

        LoggerMock.VerifyInfo(resultMessageFormat, name);
    }

    protected async Task ShouldLogImportStoppedWhenCancellationRequestedAsync(int cancellationTimeoutMilliseconds)
    {
        var cancelledMessageResourceKey = "ServiceWasCancelled";
        var cancelledMessageFormat = LogResourceManager.GetString(cancelledMessageResourceKey)
            ?? throw new InvalidOperationException($"message format {cancelledMessageResourceKey} is null or not found");

        var name = $"{Guid.NewGuid()}";
        var service = CreateService(name);

        LoaderMock.Setup(l => l.Name).Returns($"{Guid.NewGuid()}");
        LoaderMock.SetupStartSuccess();

        using var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(TimeSpan.FromMilliseconds(cancellationTimeoutMilliseconds));

        await service.Start(tokenSource.Token);

        LoggerMock.VerifyWarning(cancelledMessageFormat, name);
    }

    protected async Task ShouldLogResettingErrorIfLoaderServiceNeedsResettingAsync(int cancellationTimeoutMilliseconds)        
    {
        var messageResourceKey = "ImportWasStoppedLoaderServiceAlreadyReseted";
        var resultMessageFormat = LogResourceManager.GetString(messageResourceKey) 
            ?? throw new InvalidOperationException($"message format {messageResourceKey} is null or not found");

        var service = CreateService($"{Guid.NewGuid()}");

        LoaderMock.Setup(l => l.Name).Returns($"{Guid.NewGuid()}");
        LoaderMock.SetupStartSuccess();

        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);

        var loaderServiceException = new ResetLoaderServiceException("loading failed.");
        LoaderMock.Setup(l => l.Load(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>())).ThrowsAsync(loaderServiceException);

        using var tokenSource = new CancellationTokenSource();
        
        await Task.WhenAny(service.Start(tokenSource.Token), Task.Delay(cancellationTimeoutMilliseconds));

        LoaderMock.Verify(l => l.Start(It.IsAny<CancellationToken>()), Times.Once);

        LoggerMock.VerifyError(loaderServiceException, resultMessageFormat, LoaderMock.Object.Name, service.Name);
    }


    protected async Task ShouldLogServiceFailedErrorWhenUnhandledExceptionThrownAsync(int cancellationTimeoutMilliseconds)
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
        
        await Task.WhenAny(service.Start(tokenSource.Token), Task.Delay(cancellationTimeoutMilliseconds));

        LoaderMock.Verify(l => l.Start(It.IsAny<CancellationToken>()), Times.Once);

        LoggerMock.VerifyError(exception, (v,e)=>v.InnerException == e, resultMessageFormat, service.Name);
    }

    protected async Task ShouldLogRequestFailedAndLoaderWillBePausedWarningWhenLoaderNeedsWaitAsync(int cancellationTimeoutMilliseconds)
    {
        var messageResourceKey = "RequestFailedAndLoaderWillBePaused";
        var resultMessageFormat = LogResourceManager.GetString(messageResourceKey)
            ?? throw new InvalidOperationException($"message format {messageResourceKey} is null or not found");

        var innerException = new Exception("status code 403");
        var retryAfter = TimeSpan.FromMilliseconds(2000);
        var exception = new RetryAfterLoaderServiceException("request forbidden", innerException, retryAfter);

        LoaderMock.Setup(l => l.Name).Returns($"{Guid.NewGuid()}");
        LoaderMock.SetupStartSuccess();
        LoaderMock.Setup(w => w.Load(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>())).ThrowsAsync(exception);

        var service = CreateService($"{Guid.NewGuid()}");

        using var tokenSource = new CancellationTokenSource();

        await Task.WhenAny(service.Start(tokenSource.Token), Task.Delay(cancellationTimeoutMilliseconds));

        LoaderMock.Verify(l => l.Start(It.IsAny<CancellationToken>()), Times.Once);

        LoggerMock.VerifyWarning(exception, resultMessageFormat);
    }
}