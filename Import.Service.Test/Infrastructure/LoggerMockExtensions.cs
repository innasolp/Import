using Microsoft.Extensions.Logging;
using Moq;

namespace Import.Service.Test.Infrastructure;

public static class LoggerMockExtensions
{
    private static bool LogStateComparer(object state, string messageFormat, params object[] args)
    {
        if (state is IReadOnlyList<KeyValuePair<string, object>> stateDictionary)
        {
            return stateDictionary.Any(s => s.Value.ToString() == messageFormat)
                && (args.Length == 0 || args.All(arg => stateDictionary.Any(s => s.Value.Equals(arg))));
        }
        else
            return state.ToString() == string.Format(messageFormat, args);
    }

    private static void VerifyMessage<TLogger>(this Mock<TLogger> loggerMock, LogLevel logLevel, Exception? e, string messageFormat, params object[] args)
        where TLogger : class, ILogger
    {
        loggerMock.Verify(l => l.Log(
           It.Is<LogLevel>(v => v == logLevel),
           It.IsAny<EventId>(),
           It.Is<It.IsAnyType>((v, t) => LogStateComparer(v, messageFormat, args)),
           It.Is<Exception?>(v => v == null && e == null || v == e),
           It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    }   

    private static void VerifyMessage<TLogger>(this Mock<TLogger> loggerMock, LogLevel logLevel, Exception? e, Func<Exception, Exception, bool> compareException, string messageFormat, params object[] args)
        where TLogger : class, ILogger
    {
        loggerMock.Verify(l => l.Log(
           It.Is<LogLevel>(v => v == logLevel),
           It.IsAny<EventId>(),
           It.Is<It.IsAnyType>((v, t) => LogStateComparer(v, messageFormat, args)),
           It.Is<Exception?>(v => compareException(v, e)),
           It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    }

    public static void VerifyInfo<TLogger>(this Mock<TLogger> loggerMock, string messageFormat, params object[] args)
        where TLogger : class, ILogger
    {
        loggerMock.VerifyMessage(LogLevel.Information, null, messageFormat, args);
    }

    public static void VerifyWarning<TLogger>(this Mock<TLogger> loggerMock, string messageFormat, params object[] args)
        where TLogger : class, ILogger
    {
        loggerMock.VerifyMessage(LogLevel.Warning, It.IsAny<Exception?>(), messageFormat, args);
    }

    public static void VerifyWarning<TLogger>(this Mock<TLogger> loggerMock, Exception? exception, string messageFormat, params object[] args)
        where TLogger : class, ILogger
    {
        loggerMock.VerifyMessage(LogLevel.Warning, exception, messageFormat, args);
    }


    public static void VerifyError<TLogger>(this Mock<TLogger> loggerMock, Exception? exception, string messageFormat, params object[] args)
        where TLogger : class, ILogger
    {
        loggerMock.VerifyMessage(LogLevel.Error, exception, messageFormat, args);
    }

    public static void VerifyError<TLogger>(this Mock<TLogger> loggerMock, Exception? exception, Func<Exception, Exception, bool> compareException,
          string messageFormat, params object[] args)
        where TLogger : class, ILogger
    {
        loggerMock.VerifyMessage(LogLevel.Error, exception, compareException, messageFormat, args);
    }
}