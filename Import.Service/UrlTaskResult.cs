using Import.Interfaces;

namespace Import.Service;

public readonly struct UrlTaskResult<T>
{
    public ResultStatus Status { get; }

    public T? Value { get; }

    public Exception? Exception { get; }

    public string Url { get; }

    private UrlTaskResult(T? value, string url, ResultStatus result, Exception? exception = null)
    {
        Status = result;
        Value = value;
        Exception = exception;
        Url = url;
    }

    public static UrlTaskResult<T> Success(T? value, string url)
    {
        return new UrlTaskResult<T>(value, url, ResultStatus.Success);
    }

    public static UrlTaskResult<T> Warning(T? value, string url, Exception? exception = null)
    {
        return new UrlTaskResult<T>(value, url, ResultStatus.Warning, exception);
    }

    public static UrlTaskResult<T> Failed(T? value, string url, Exception exception)
    {
        return new UrlTaskResult<T>(value, url, ResultStatus.Error, exception);
    }

    public static UrlTaskResult<T> Cancelled(string url)
    {
        return new UrlTaskResult<T>(default, url, ResultStatus.Cancelled, null);
    }

    public static UrlTaskResult<T> FromStatus(ResultStatus status, string url, Exception exception)
    {
        return new UrlTaskResult<T>(default, url, status, exception);
    }
}
