
namespace Import.Interfaces;

public class ConnectedAsyncEventArgs(bool connected, Exception? exception = null, CancellationToken cancellationToken = default)
    : AsyncEventArgs(exception, cancellationToken)
{
    public bool Connected { get; } = connected;
}

public interface IImportService
{
    Task Start(CancellationToken stoppingToken);

    Task Stop(CancellationToken stoppingToken);

    string Name { get; }

    event AsyncEventHandler<ConnectedAsyncEventArgs> ConnectedAsync;

    bool Connected { get; }
}
