using Import.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;
using Import.Service.Test.Infrastructure;

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

        var task = service.StartServiceInFactoryAsync(token.Token);

        await Task.Delay(2000);

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
     
        await Task.Delay(1000);        

        await task.WaitAsync(token.Token); 

        LoggerMock.VerifyInfo(LogResourceManager.GetString("ServiceWasStopped"), name);
    }

    protected async Task ImportFailedWhenLoaderAlwaysNeedResetingAsync()        
    {
        var name = Guid.NewGuid().ToString();
        var service = CreateService(name);

        LoaderMock.Setup(l => l.Name).Returns(Guid.NewGuid().ToString());
        LoaderMock.SetupStartSuccess();

        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);

        LoaderMock.Setup(l => l.Load(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>())).Returns(
            (string url, object? data, CancellationToken token) =>
                {
                    throw new LoaderServiceException($"{url} failed.", LoaderServiceAction.Reset);
                }
            );

        var token = new CancellationTokenSource();
        var exception = await Assert.ThrowsAsync<Exception>(async ()=> await service.Start(token.Token));            
    }
}
