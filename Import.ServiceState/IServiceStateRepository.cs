using Import.Interfaces;

namespace Import.ServiceState;

public interface IServiceStateRepository : IAsyncDisposable
{
    Task Connect(CancellationToken cancellationToken = default);

    bool IsConnected { get; }

    Task<T?> Load<T>(byte[] key, CancellationToken cancellationToken = default);

    Task Save<T>(byte[] key, T value, CancellationToken cancellationToken = default);

    Task Remove(byte[] key, CancellationToken cancellationToken = default);

    Task Close(CancellationToken cancellationToken = default);

    event AsyncEventHandler<ConnectedAsyncEventArgs> ConnectedAsync;
}