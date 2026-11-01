using Import.Catalog.Category.Interfaces;
using Import.Factory.Interfaces;
using Import.Settings.Interfaces;

namespace Import.Catalog.Category.Factory.Interfaces;

public interface ICategoryItemHandlerFactory<TCategory> : IFactory
{
    ICategoryItemHandler<TCategory> Create(IImportSettingsProvider importSettingsProvider,
     IImportSettings importSettings);
}