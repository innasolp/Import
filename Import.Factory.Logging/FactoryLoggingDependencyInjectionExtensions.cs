using Import.Factory.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Import.Factory.Logging;

public static class FactoryLoggingDependencyInjectionExtensions
{
    public static IServiceCollection AddImportServiceLogFactory(this IServiceCollection services, Func<ILogger, string, IImportSource, IImportSettings, ILogger> getLoggerForSource)
    {
        return services.AddSingleton<IImportServiceLogFactory>(new ImportServiceLogFactoryImpl(getLoggerForSource));
    }
}
