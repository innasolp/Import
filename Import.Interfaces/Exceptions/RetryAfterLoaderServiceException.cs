namespace Import.Interfaces.Exceptions;

public class RetryAfterLoaderServiceException : LoaderServiceException
{
    public RetryAfterLoaderServiceException(TimeSpan retryAfter)
    {
        RetryAfter = retryAfter;
    }

    public RetryAfterLoaderServiceException(string? message, TimeSpan retryAfter) : base(message)
    {
        RetryAfter = retryAfter;
    }

    public RetryAfterLoaderServiceException(string? message, Exception? innerException, TimeSpan retryAfter) : base(message, innerException)
    {
        RetryAfter = retryAfter;
    }

    public override LoaderServiceAction? NeedsAction => LoaderServiceAction.RetryAfter;

    public TimeSpan RetryAfter { get; }
}