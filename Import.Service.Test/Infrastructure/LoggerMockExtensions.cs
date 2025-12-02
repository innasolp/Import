using Microsoft.Extensions.Logging;
using Moq;

namespace Import.Service.Test.Infrastructure;

public static class LoggerMockExtensions
{
    private static void VerifyMessage<TLogger>(this Mock<TLogger> loggerMock, LogLevel logLevel, Exception? e, string messageFormat, params object[] args)
        where TLogger : class, ILogger
    {
        var message = string.Format(messageFormat, args ?? []);

        loggerMock.Verify(l => l.Log(
           It.Is<LogLevel>(v => v == logLevel),
           It.IsAny<EventId>(),
           It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
           It.Is<Exception?>(v=> v == null && e == null || v == e),
           It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
    }

    public static void VerifyInfo<TLogger>(this Mock<TLogger> loggerMock, string messageFormat, params object[] args)
        where TLogger : class, ILogger
    {
        loggerMock.VerifyMessage(LogLevel.Information, null, messageFormat, args);
    }

    public static void VerifyInfo<TLogger>(this Mock<TLogger> loggerMock, string message)
        where TLogger : class, ILogger
    {
        loggerMock.VerifyInfo(message, null);
    }

    public static void VerifyWarning<TLogger>(this Mock<TLogger> loggerMock, string messageFormat, params object[] args)
        where TLogger : class, ILogger
    {
        loggerMock.VerifyMessage(LogLevel.Warning, null, messageFormat, args);
    }

    public static void VerifyWarning<TLogger>(this Mock<TLogger> loggerMock, Exception? exception, string messageFormat, params object[] args)
        where TLogger : class, ILogger
    {
        loggerMock.VerifyMessage(LogLevel.Warning, exception, messageFormat,args);
    }

    public static void VerifyError<TLogger>(this Mock<TLogger> loggerMock, Exception? exception, string messageFormat, params object[] args)
        where TLogger : class, ILogger
    {
        loggerMock.VerifyMessage(LogLevel.Error, exception, messageFormat, args);
    }
    
    public static void VerifyError<TLogger>(this Mock<TLogger> loggerMock, string messageFormat, params object[] args)
        where TLogger : class, ILogger
    {
        loggerMock.VerifyMessage(LogLevel.Error, null, messageFormat, args);
    }
}
