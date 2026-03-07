namespace Import.Interfaces;

public delegate Task AsyncEventHandler(object sender, AsyncEventArgs eventArgs);

public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs eventArgs)
    where TEventArgs : AsyncEventArgs;

public class AsyncEventArgs(Exception? exception = null, CancellationToken cancellationToken = default) : EventArgs
{
    public Exception? Exception { get; } = exception;

    public CancellationToken CancellationToken { get; } = cancellationToken;
}