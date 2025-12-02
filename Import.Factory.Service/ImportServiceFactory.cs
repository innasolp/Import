using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;
using Import.Factory.Interfaces;
using Import.Interfaces;

namespace Import.Factory.Service;

public abstract class ImportServiceFactory(ILogger logger,
    ILoaderServiceFactory browserServiceFactory,   
    IImportServiceLogFactory? logFactory=null) : IImportServiceFactory
{
    private readonly ILogger _logger = logger;

    private readonly ILoaderServiceFactory _browserServiceFactory = browserServiceFactory;

    private readonly IImportServiceLogFactory? _logFactory = logFactory;    

    public abstract Type ServiceImplementationType { get; }

    IImportService IImportServiceFactory.Create(string name, IImportSource shopModel, IShopImportSettings shopImportSettings)
    {
        var browserService = _browserServiceFactory.Create(name, shopImportSettings);   

        var logger = _logFactory == null ? _logger : GetLogger( _logger, name, _logFactory, shopModel, shopImportSettings) ?? _logger;

        return Create(logger, name, shopModel, shopImportSettings, browserService);
    }

    protected abstract ILogger GetLogger(ILogger logger, string name, IImportServiceLogFactory importServiceLogFactory, IImportSource shopModel, IShopImportSettings shopImportSettings);

    protected abstract IImportService Create(ILogger logger,
        string name,
        IImportSource shopModel, 
        IShopImportSettings shopImportSettings,  
        ILoaderService browserService);
}
