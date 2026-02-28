using Moq;
using System.Text.Json;
using Import.Interfaces;

namespace Import.Service.Test.Infrastructure;

public static class TestExtensions
{
    public static void SetupLoadCookies(this Mock<ILoaderService> loaderMock)
    {
        loaderMock.Setup(w => w.GetData(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(async (string host, CancellationToken token) => await Task.FromResult(new object()));
    }

    public static void SetupStartSuccess(this Mock<ILoaderService> loaderMock)
    {
        loaderMock.Setup(w => w.Start(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Callback((CancellationToken token) => loaderMock.Setup(w => w.IsStarted).Returns(true));
    }

    public static void SetupGetRequestData(this Mock<ILoaderService> loaderMock,
        object requestData)
    {
        loaderMock.Setup(l => l.GetData(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(requestData));
    }

    public static void SetupLoadItem<T>(this Mock<ILoaderService> loaderMock,
        string itemUrl, 
        object requestData,
        T item, 
        Func<CancellationToken, Task>? onLoad = null)
        where T:class
    {
        loaderMock.Setup(w => w.Load(itemUrl, requestData, It.IsAny<CancellationToken>())).Returns(
            (string url, object requestData, CancellationToken token) => LoadItemAsync(item, token, onLoad));
    }

    public static void SetupLoadItem<T>(this Mock<ILoaderService> loaderMock,
        string itemUrl,
        object requestData,
        T item,
        Func<T, CancellationToken, Task<Stream>> loadItem)
        where T : class
    {
        loaderMock.Setup(w => w.Load(itemUrl, requestData, It.IsAny<CancellationToken>())).Returns(
            (string url, object requestData, CancellationToken token) => loadItem(item, token));
    }


    public static void SetupLoadItemsSuccessfull<T>(this Mock<ILoaderService> loaderMock,
        Dictionary<string,T> itemUrls, object requestData, Func<CancellationToken,Task>? onLoad = null)
        where T : class
    {
        foreach(var itemUrl in itemUrls)
        loaderMock.Setup(w => w.Load(itemUrl.Key, requestData, It.IsAny<CancellationToken>()))
                .Returns((string url, object? data, CancellationToken token)=> LoadItemAsync(itemUrl.Value, token, onLoad));
    }

    public static void SetupLoadItemsThrowsExceptions(this Mock<ILoaderService> webLoaderMock,
        IEnumerable<string> itemUrls, 
        Func<string, Exception> getItemException,
        object requestData)
    {
        foreach (var itemUrl in itemUrls)
            webLoaderMock.SetupLoadItemThrowsException(itemUrl, getItemException(itemUrl), requestData);
    }

    public static void SetupLoadItemThrowsException(this Mock<ILoaderService> webLoaderMock,
        string itemUrl,
        Exception exception,
        object requestData)
    {
        webLoaderMock.Setup(w => w.Load(itemUrl, requestData, It.IsAny<CancellationToken>())).ThrowsAsync(exception);
    }

    public static async Task<Stream> LoadItemAsync<T>(T item, CancellationToken cancellationToken, Func<CancellationToken, Task>? onLoad = null)
        where T : class
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(item);
        var fs = new MemoryStream(bytes);
        if (onLoad != null)
            await onLoad(cancellationToken);
        return await Task.FromResult(fs);
    }

    public static Task StartServiceInFactoryAsync(this IImportService service, CancellationToken token)
    {
        return Task.Factory.StartNew(async () => await service.Start(token),token,
            TaskCreationOptions.RunContinuationsAsynchronously,            
           TaskScheduler.Current);
    }

    public static void VerifyLoadUrlAndRequestHeaders(this Mock<ILoaderService> loaderMock, 
        string url, 
        object requestData)
    {
        loaderMock.Verify(l => l.Load(It.Is<string>(v => v == url), 
            It.Is<object>(r=>r == requestData), It.IsAny<CancellationToken>()));
    }
}
