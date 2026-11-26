using Moq;
using System.Text.Json;
using Import.Interfaces;

namespace Import.Service.Test.Infrastructure;

public static class TestExtensions
{
    public static void SetupStartSuccess<TLoadData>(this Mock<ILoaderService<TLoadData>> loaderMock)
    {
        loaderMock.Setup(w => w.Start(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Callback((CancellationToken token) => loaderMock.Setup(w => w.IsStarted).Returns(true));
    }

    public static void SetupLoadItem<T, TLoadData>(this Mock<ILoaderService<TLoadData>> loaderMock,
        string itemUrl,
        T item,
        Func<T, CancellationToken, Task>? onLoad = null)
        where T : class
    {
        loaderMock.Setup(w => w.Load(itemUrl, It.IsAny<TLoadData?>(), It.IsAny<CancellationToken>())).Returns(
            (string url, TLoadData? data, CancellationToken token) => LoadItemAsync(item, token, onLoad));
    }

    public static void SetupLoadItem<T, TLoadData>(this Mock<ILoaderService<TLoadData>> loaderMock,
        string itemUrl,
        T item,
        Func<T, CancellationToken, Task<Stream>> loadItem)
        where T : class
    {
        loaderMock.Setup(w => w.Load(itemUrl, It.IsAny<TLoadData?>(), It.IsAny<CancellationToken>())).Returns(
            (string url, TLoadData? data, CancellationToken token) => loadItem(item, token));
    }


    public static void SetupLoadItemsSuccessfull<T,TLoadData>(this Mock<ILoaderService<TLoadData>> loaderMock,
        Dictionary<string, T> itemUrls, Func<T, CancellationToken, Task>? onLoad = null)
        where T : class
    {
        foreach (var itemUrl in itemUrls)
            loaderMock.Setup(w => w.Load(itemUrl.Key, It.IsAny<TLoadData?>(), It.IsAny<CancellationToken>()))
                    .Returns((string url, TLoadData? data, CancellationToken token) => LoadItemAsync(itemUrl.Value, token, onLoad));
    }

    public static void SetupLoadItemsThrowsExceptions<TLoadData>(this Mock<ILoaderService<TLoadData>> webLoaderMock,
        IEnumerable<string> itemUrls,
        Func<string, Exception> getItemException,
        TLoadData? requestData)
    {
        foreach (var itemUrl in itemUrls)
            webLoaderMock.SetupLoadItemThrowsException(itemUrl, getItemException(itemUrl), requestData);
    }

    public static void SetupLoadItemThrowsException<TLoadData>(this Mock<ILoaderService<TLoadData>> webLoaderMock,
        string itemUrl,
        Exception exception,
        TLoadData? requestData)
    {
        webLoaderMock.Setup(w => w.Load(itemUrl, requestData, It.IsAny<CancellationToken>())).
            ThrowsAsync(exception);
    }

    public static async Task<Stream> LoadItemAsync<T>(T item, CancellationToken cancellationToken, Func<T, CancellationToken, Task>? onLoad = null)
        where T : class
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(item);
        var fs = new MemoryStream(bytes);
        if (onLoad != null)
            await onLoad(item, cancellationToken);
        return await Task.FromResult(fs);
    }

    public static Task StartServiceInFactoryAsync(this IService service, CancellationToken token)
    {
        return Task.Factory.StartNew(async () => await service.Start(token), token,
            TaskCreationOptions.RunContinuationsAsynchronously,
           TaskScheduler.Current);
    }

    public static void VerifyLoadUrlAndRequestHeaders<TLoadData>(this Mock<ILoaderService<TLoadData>> loaderMock,
        string url,
        TLoadData? requestData)
        where TLoadData: class
    {
        loaderMock.Verify(l => l.Load(It.Is<string>(v => v == url),
            It.Is<TLoadData?>(r => r == requestData), It.IsAny<CancellationToken>()));
    }
}
