using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;

namespace Import.Factory.Service;

internal class ImportServiceFactory(IEnumerable<IServiceFactory> serviceFactories) : IImportServiceFactory
{
    private const string ImportService = "ImportService";

    public IService Create(string name, IImportSource importSource, IImportSettingsProvider importSettingsProvider, IImportSettings importSettings)
    {
        var serviceFactory = serviceFactories.GetFactoryBySettings(importSettingsProvider, importSettings, ImportService, importSource.Name);
        return serviceFactory.Create(name, importSource, importSettingsProvider, importSettings);
    }
}