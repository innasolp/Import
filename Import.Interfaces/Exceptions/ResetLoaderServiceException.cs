namespace Import.Interfaces.Exceptions;

public class ResetLoaderServiceException : LoaderServiceException
{
    public ResetLoaderServiceException()
    {
    }

    public ResetLoaderServiceException(string? message) : base(message)
    {
    }

    public ResetLoaderServiceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public override LoaderServiceAction? NeedsAction => LoaderServiceAction.Reset;
}