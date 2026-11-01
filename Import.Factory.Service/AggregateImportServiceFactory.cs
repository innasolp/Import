using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Service;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Factory.Service;

internal class AggregateServiceFactory(ILogger<AggregateService> logger, IImportServiceLogFactory? logFactory = null)
: IAggregateServiceFactory
{
    private readonly ILogger<AggregateService> _logger = logger;
    private readonly IImportServiceLogFactory? _logFactory = logFactory;

    private readonly IImportServiceFactory? _importServiceFactory;

    public AggregateServiceFactory(ILogger<AggregateService> logger, IImportServiceFactory importServiceFactory, IImportServiceLogFactory? logFactory = null)
    : this(logger, logFactory)
    {
        _importServiceFactory = importServiceFactory;
    }

    public IAggregateImportService Create(string name,
     IImportSource importSource,
      IImportSettingsProvider importSettingsProvider,
      IImportSettings importSettings,
      IEnumerable<IService> services,
      Func<IService, CancellationToken, Task>? executionTask = null)
    {
        var serviceLogger = _logFactory?.GetLogger(_logger, name, importSource, importSettings) ?? _logger;
        return new AggregateService(serviceLogger, name, services, executionTask);
    }

    public IAggregateImportService Create(string name,
    ISplittableImportSource importSource,
     IImportSettingsProvider importSettingsProvider,
      IImportSettings importSettings,
      Func<IImportSource, string>? getName = null,
      Func<IService, CancellationToken, Task>? executionTask = null)
    {
        if (_importServiceFactory == default)
            throw new Exception("ImportServiceFactory is not set");

        var sources = importSource.Split().ToList();
        var services = new List<IService>(sources.Count);

        foreach (var source in sources)
            services.Add(_importServiceFactory.Create(getName?.Invoke(source) ?? $"{name}/{source.Name}",
                        source, importSettingsProvider, importSettings));

        return Create(name, importSource, importSettingsProvider, importSettings, services, executionTask);
    }
}