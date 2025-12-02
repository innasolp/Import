using Import.Factory.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Factory.Logging;

internal class ImportServiceLogFactoryImpl(Func<ILogger, string, IImportSource, IShopImportSettings, ILogger> loggerInterception) : IImportServiceLogFactory
{
    private readonly Func<ILogger, string, IImportSource, IShopImportSettings, ILogger> _loggerInterception = loggerInterception;    

    ILogger<T> IImportServiceLogFactory.GetLogger<T>(ILogger<T> logger, string name, IImportSource shopModel, IShopImportSettings shopImportSettings)
    {
        var interceptedLogger = _loggerInterception(logger, name, shopModel, shopImportSettings);
        return new LogEmptyInterceptorImpl<T>(interceptedLogger);
    }
}