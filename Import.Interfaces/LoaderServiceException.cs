namespace Import.Interfaces;

public enum LoaderServiceAction
{
    Reset = 0,
    Wait = 1,
    Stop = 2
}

public class LoaderServiceException : Exception
{
    public LoaderServiceAction? NeedAction { get; }

    public LoaderServiceException(LoaderServiceAction? needReset = null)
    {
        NeedAction = needReset;
    }

    public LoaderServiceException(string? message, LoaderServiceAction? needReset = null) : base(message)
    {
        NeedAction = needReset;
    }

    public LoaderServiceException(string? message, Exception? innerException, LoaderServiceAction? needReset = null) : base(message, innerException)
    {
        NeedAction = needReset;
    }
}
