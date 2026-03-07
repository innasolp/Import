namespace Import.Interfaces.Exceptions;

public enum LoaderServiceAction
{
    None = 0,
    Reset = 1,
    RetryAfter = 2
}

public abstract class LoaderServiceException : Exception
{
    public abstract LoaderServiceAction? NeedsAction { get; }

    public LoaderServiceException()
    {        
    }

    public LoaderServiceException(string? message) : base(message)
    {        
    }

    public LoaderServiceException(string? message, Exception? innerException) : base(message, innerException)
    {       
    }
}