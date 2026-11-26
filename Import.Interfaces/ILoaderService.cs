namespace Import.Interfaces;

public class LoadData<TOptions>
{
    public TOptions? Options { get; set; }

    public string[]? Parameters { get; set; }
}

public interface ILoaderService<TData> : IAsyncDisposable
{
    string Name { get; }

    Task Start(CancellationToken cancellationToken = default);

    Task Reset(CancellationToken cancellationToken = default);

    Task Close(CancellationToken cancellationToken = default);

    bool IsStarted { get; }

    Task<Stream> Load(string url, TData? data, CancellationToken cancellationToken = default);
}