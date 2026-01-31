
namespace Import.Interfaces;

public class ConnectedAsyncEventArgs(bool success, Exception? exception = null, CancellationToken cancellationToken = default)
    : AsyncEventArgs(exception, cancellationToken)
{
    public bool Success { get; } = success;
}

public interface IImportService
{
    Task Start(CancellationToken stoppingToken);

    Task Stop(CancellationToken stoppingToken);

    string Name { get; }

    event AsyncEventHandler<ConnectedAsyncEventArgs> ConnectedAsync;
}
