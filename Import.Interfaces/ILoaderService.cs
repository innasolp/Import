namespace Import.Interfaces;

public interface ILoaderService : IAsyncDisposable
{
    string Name { get; }

    Task Start(CancellationToken cancellationToken = default);

    Task Reset(CancellationToken cancellationToken = default);

    Task Close(CancellationToken cancellationToken = default);

    bool IsStarted { get; }

    Task<object> GetData(string host, CancellationToken cancellationToken = default);

    Task UpdateData(string url, CancellationToken cancellationToken = default);

    Task<Stream> Load(string url, object? data, CancellationToken cancellationToken = default);
}
