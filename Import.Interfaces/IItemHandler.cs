namespace Import.Interfaces;

public class ItemProcessEventArgs<TItem, TStatus>(TItem item, TStatus processStatus, Exception? exception = null, CancellationToken cancellationToken = default)
: AsyncEventArgs(exception, cancellationToken)
{
    public TStatus ProcessStatus { get; } = processStatus;

    public TItem Item { get; } = item;
}


public interface IItemHandler<TItem, TStatus>
{
    Task<TStatus> HandleItem(TItem item, CancellationToken cancellationToken = default);

    event AsyncEventHandler<ItemProcessEventArgs<TItem, TStatus>> ItemProcessed;
}