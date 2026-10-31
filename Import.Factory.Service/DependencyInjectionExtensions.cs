using Import.Factory.Interfaces;
using Import.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Import.Factory.Service;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddImportServiceFactory(this IServiceCollection services)
    {
        return services.AddScoped<IImportServiceFactory>(sd =>
        {
            var serviceFactories = sd.GetServices<IServiceFactory>();
            return new ImportServiceFactory(serviceFactories);
        });
    }

    public static IServiceCollection AddAggregateServiceFactory(this IServiceCollection services)
    {
        return services.AddScoped<IAggregateServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<AggregateService>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            return new AggregateServiceFactory(logger, loggerFactory);
        });
    }
}