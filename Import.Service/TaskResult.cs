using Import.Interfaces;

namespace Import.Service;

public readonly struct TaskResult
{
    public ResultStatus Status { get; }

    public Exception? Exception { get; }

    public TaskResult() { Status = ResultStatus.Error; }

    private TaskResult(ResultStatus result, Exception? exception = null)
    {
        Status = result;
        Exception = exception;
    }

    public static TaskResult Success()
    {
        return new TaskResult(ResultStatus.Success);
    }

    public static TaskResult Warning(Exception? exception = null)
    {
        return new TaskResult(ResultStatus.Warning, exception);
    }

    public static TaskResult Failed(Exception exception)
    {
        return new TaskResult(ResultStatus.Error, exception);
    }

    public static TaskResult Cancelled()
    {
        return new TaskResult(ResultStatus.Cancelled, null);
    }
}

public readonly struct TaskResult<T>
{
    public ResultStatus Status { get; }

    public T? Value { get; }

    public Exception? Exception { get; }

    public TaskResult() { Status = ResultStatus.Error; }

    private TaskResult(T? value, ResultStatus result, Exception? exception = null)
    {
        Status = result;
        Value = value;
        Exception = exception;
    }

    public static TaskResult<T> Success(T? value)
    {
        return new TaskResult<T>(value, ResultStatus.Success);
    }

    public static TaskResult<T> Warning(T? value, Exception? exception = null)
    {
        return new TaskResult<T>(value, ResultStatus.Warning, exception);
    }

    public static TaskResult<T> Failed(T? value, Exception exception)
    {
        return new TaskResult<T>(value, ResultStatus.Error, exception);
    }

    public static TaskResult<T> Failed(Exception exception)
    {
        return new TaskResult<T>(default(T), ResultStatus.Error, exception);
    }
    
    public static TaskResult<T> Cancelled()
    {
        return new TaskResult<T>(default(T), ResultStatus.Cancelled, null);
    }

    public static TaskResult<T> FromStatus(ResultStatus status, Exception exception)
    {
        return new TaskResult<T>(default(T), status, exception);
    }

    public static TaskResult<T> FromStatus(ResultStatus status)
    {
        return new TaskResult<T>(default(T), status, null);
    }
}
