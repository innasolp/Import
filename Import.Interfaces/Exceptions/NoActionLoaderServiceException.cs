namespace Import.Interfaces.Exceptions;

public class NoActionLoaderServiceException : LoaderServiceException
{
    public NoActionLoaderServiceException()
    {
    }

    public NoActionLoaderServiceException(string? message) : base(message)
    {
    }

    public NoActionLoaderServiceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public override LoaderServiceAction? NeedsAction => null;
}