namespace Import.Service;

[Serializable]
public class ImportWarningException : Exception
{
    public object? Result { get; }

    public ImportWarningException(object? result) : base() { Result = result; }

    public ImportWarningException(string message, object? result = null) : base(message) { Result = result; }

    public ImportWarningException(string? message, Exception? innerException, object? result) : base(message, innerException)
    {
        Result = result;
    }
}