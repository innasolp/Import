using Import.Settings.Interfaces;
using Import.Interfaces;

namespace Import.Factory.Interfaces;

public interface IImportServiceFactory  
{
    Type ServiceImplementationType { get; }

    IImportService Create(string name, IImportSource shopModel, IShopImportSettings shopImportSettings);
}
