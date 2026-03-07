using Import.Settings.Interfaces;
using Import.Interfaces;

namespace Import.Factory.Interfaces;

public interface ILoaderServiceFactory
{
    ILoaderService Create(string name, IImportSettings importSettings);
}