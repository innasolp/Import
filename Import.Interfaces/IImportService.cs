
namespace Import.Interfaces;

public class ConnectedAsyncEventArgs(bool success, object? parameter, Exception? exception = null, CancellationToken cancellationToken = default)
    : AsyncEventArgs(exception, cancellationToken)
{
    public object? Parameter { get; } = parameter;

    public bool Success { get; } = success;
}

public interface IImportService
{
    Task Start(object? parameter, CancellationToken stoppingToken);

    string Name { get; }

    bool IsStarted { get; }

    event AsyncEventHandler<ConnectedAsyncEventArgs> ConnectedAsync;
}
