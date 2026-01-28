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
    protected async Task ImportWasStoppedWhenLoaderNotExecutedAsync()
    {
        
        var exception = new InvalidOperationException("test fatal error");
        var name = Guid.NewGuid().ToString();
        var service = CreateService(name);

        LoaderMock.Setup(l => l.Name).Returns(Guid.NewGuid().ToString());
        LoaderMock.Setup(w => w.Start(It.IsAny<CancellationToken>())).Throws(exception);

        var token = new CancellationTokenSource();
        await service.Start(null, token.Token);

        LoggerMock.VerifyInfo(LogResourceManager.GetString("ServiceWasStopped"), name);

        LoggerMock.VerifyError(exception, LogResourceManager.GetString("ImportWasStoppedWebLoaderNotExecute"), LoaderMock.Object.Name);
    }

    protected async Task ImportStartedWhenLoaderExecutedSuccessfullAsync()
    {        
        var name = Guid.NewGuid().ToString();
        var service = CreateService(name);

        LoaderMock.SetupStartSuccess();

        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(500);

        await service.Start(null, tokenSource.Token);

        LoggerMock.VerifyInfo(LogResourceManager.GetString("ServiceStarted"), name);
    }

    protected async Task ImportStoppedWhenCancellationRequestedAsync()
    {
        var name = Guid.NewGuid().ToString();
        var service = CreateService(name);

        LoaderMock.SetupStartSuccess();

        var tokenSource = new CancellationTokenSource();     
        tokenSource.CancelAfter(500);

        await service.Start(null, tokenSource.Token); 

        LoggerMock.VerifyInfo(LogResourceManager.GetString("ServiceWasStopped"), name);
    }

    protected async Task LogResetingWarningIfLoaderServiceNeedResetingAsync()        
    {
        var service = CreateService($"{Guid.NewGuid()}");

        LoaderMock.Setup(l => l.Name).Returns($"{Guid.NewGuid()}");
        LoaderMock.SetupStartSuccess();

        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);

        var loaderServiceException = new LoaderServiceException("loading failed.", LoaderServiceAction.Reset);
        LoaderMock.Setup(l => l.Load(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>())).Throws(loaderServiceException);

        var token = new CancellationTokenSource();
        await service.Start(null, token.Token);

        await Task.Delay(500);

        await token.CancelAsync();

        var messagePart = $"completed with error {loaderServiceException.Message} and need in reseting";
        LoggerMock.VerifyError(loaderServiceException, LogResourceManager.GetString("ImportWasStoppedLoaderServiceAlreadyReseted"),
            LoaderMock.Object.Name, service.Name);
    }

    

    protected async Task LogServiceFailedErrorWhenUnhandledExceptionThrownAsync()
    {
        var service = CreateService($"{Guid.NewGuid()}");

        LoaderMock.Setup(l => l.Name).Returns($"{Guid.NewGuid()}");
        LoaderMock.SetupStartSuccess();

        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);

        var exception = new Exception("loading failed.");
        LoaderMock.Setup(l => l.Load(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>())).Throws(exception);

        var token = new CancellationTokenSource();
        await service.Start(null, token.Token);
        
        LoggerMock.VerifyError(exception, (v,e)=>v.InnerException == e,
            LogResourceManager.GetString("ServiceFailedWithError"), service.Name, exception.Message);
    }

    protected async Task LogRequestFailedAndLoaderWillBePausedWarningWhenForbiddenRequestAsync()
    {
        var innerException = new HttpRequestException(HttpRequestError.InvalidResponse, "request forbidden", statusCode: System.Net.HttpStatusCode.Forbidden);
        var exception = new LoaderServiceException("request forbidden", innerException, LoaderServiceAction.Wait);

        LoaderMock.Setup(l => l.Name).Returns($"{Guid.NewGuid()}");
        LoaderMock.SetupStartSuccess();
        LoaderMock.Setup(w => w.Load(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).Throws(exception);

        var service = CreateService($"{Guid.NewGuid()}");

        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(1000);

        await service.Start(new object(), tokenSource.Token);        

         var messageFormat = LogResourceManager.GetString("RequestFailedAndLoaderWillBePaused");
         LoggerMock.VerifyWarning(exception, messageFormat);
    }
}