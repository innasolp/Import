namespace Import.Service;

public class LoadFromUrlException : Exception
{
    public string Url { get; }

    public LoadFromUrlException(string url)
    {
        Url = url;
    }

    public LoadFromUrlException(string url, string? message) : base(message)
    {
        Url = url;
    }

    public LoadFromUrlException(string url, string? message, Exception? innerException) : base(message, innerException)
    {
        Url = url;
    }
}
