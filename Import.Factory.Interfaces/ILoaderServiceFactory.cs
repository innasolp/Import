using Import.Settings.Interfaces;
using Import.Interfaces;

namespace Import.Factory.Interfaces;

public interface ILoaderServiceFactory<TData> : IFactory
{
    ILoaderService<TData> Create(string name, IImportSettingsProvider importSettingsProvider, IImportSettings importSettings);
}