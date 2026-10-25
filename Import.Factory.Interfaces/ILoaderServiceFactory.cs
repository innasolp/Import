using Import.Settings.Interfaces;
using Import.Interfaces;

namespace Import.Factory.Interfaces;

public interface ILoaderServiceFactory : IFactory
{
    ILoaderService Create(string name, IImportSettings importSettings);
}