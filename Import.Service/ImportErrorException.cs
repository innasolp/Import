namespace Import.Service;

public class ImportErrorException : Exception
{
    public ImportErrorException()
    {
    }

    public ImportErrorException(string? message) : base(message)
    {
    }

    public ImportErrorException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
