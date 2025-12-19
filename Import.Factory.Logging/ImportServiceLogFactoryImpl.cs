using Import.Factory.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Factory.Logging;

internal class ImportServiceLogFactoryImpl(Func<ILogger, string, IImportSource, IImportSettings, ILogger> loggerInterception) : IImportServiceLogFactory
{
    private readonly Func<ILogger, string, IImportSource, IImportSettings, ILogger> _loggerInterception = loggerInterception;    

    ILogger<T> IImportServiceLogFactory.GetLogger<T>(ILogger<T> logger, string name, IImportSource importSource, IImportSettings importSettings)
    {
        var interceptedLogger = _loggerInterception(logger, name, importSource, importSettings);
        return new LogEmptyInterceptorImpl<T>(interceptedLogger);
    }
}