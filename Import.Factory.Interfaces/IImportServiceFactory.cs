using Import.Settings.Interfaces;
using Import.Interfaces;

namespace Import.Factory.Interfaces;

public interface IImportServiceFactory : IFactory
{
    IService Create(string name, IImportSource importSource, IImportSettings importSettings);
}