using Import.Settings.Interfaces;
using Import.Interfaces;

namespace Import.Factory.Interfaces;

public interface IServiceFactory : IFactory
{
    IService Create(string name,
     IImportSource importSource,
     IImportSettingsProvider importSettingsProvider,
     IImportSettings importSettings);
}