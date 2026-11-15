using Import.Catalog.Product.Interfaces;
using Import.Factory.Interfaces;
using Import.Settings.Interfaces;

namespace Import.Catalog.Product.Factory.Interfaces;

public interface IProductItemHandlerFactory : IFactory
{
    IProductItemHandler Create(IImportSettingsProvider importSettingsProvider, IImportSettings settings);
}