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
        await service.Start(token.Token);

        LoggerMock.VerifyInfo(LogResourceManager.GetString("ServiceWasStopped"), name);

        LoggerMock.VerifyError(exception, LogResourceManager.GetString("ImportWasStoppedWebLoaderNotExecute"), LoaderMock.Object.Name);
    }

    protected async Task ImportStartedWhenLoaderExecutedSuccessfullAsync()
    {        
        var name = Guid.NewGuid().ToString();
        var service = CreateService(name);

        LoaderMock.SetupStartSuccess();

        var token = new CancellationTokenSource();
        await service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay(500);

        LoggerMock.VerifyInfo(LogResourceManager.GetString("ServiceStarted"), name);
    }

    protected async Task ImportStoppedWhenCancellationRequestedAsync()
    {
        var name = Guid.NewGuid().ToString();
        var service = CreateService(name);

        LoaderMock.SetupStartSuccess();

        var token = new CancellationTokenSource();      

        var task = service.Start(token.Token);

        await Task.Delay(500);        

        await token.CancelAsync();
     
        await Task.Delay(500);        

        await task.WaitAsync(token.Token); 

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
        await service.Start(token.Token);

        await Task.Delay(500);

        await token.CancelAsync();

        var messagePart = $"completed with error {loaderServiceException.Message} and need in reseting";
        LoggerMock.VerifyWarning(messagePart, (v,m) => v.Contains(m));
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
        await service.Start(token.Token);
        
        LoggerMock.VerifyError(exception, (v,e)=>v.InnerException == e,
            string.Format(LogResourceManager.GetString("ServiceFailedWithError"), [service.Name, exception.Message]), (v,m)=>v == m);
    }

    protected async Task LogRequestFailedAndLoaderWillBePausedWarningWhenForbiddenRequestAsync()
    {
        var innerException = new HttpRequestException(HttpRequestError.InvalidResponse, "request forbidden", statusCode: System.Net.HttpStatusCode.Forbidden);
        var exception = new LoaderServiceException("request forbidden", innerException, LoaderServiceAction.Wait);

        LoaderMock.Setup(l => l.Name).Returns($"{Guid.NewGuid()}");
        LoaderMock.SetupStartSuccess();
        LoaderMock.Setup(w => w.Load(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>())).Throws(exception);

        var service = CreateService($"{Guid.NewGuid()}");

        var token = new CancellationTokenSource();
        var task = service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay(500);

        var messagePart = "Loader will be paused";
        LoggerMock.VerifyWarning(exception, (v,e) => v==e, messagePart, (v,m)=>v.Contains(messagePart));

        await token.CancelAsync();
    }
}