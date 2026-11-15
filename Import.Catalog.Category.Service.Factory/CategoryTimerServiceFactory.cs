using Import.Catalog.Category.Factory.Interfaces;
using Import.Catalog.Category.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

public abstract class CategoryTimerServiceFactory<TCategoryTimerService, TTreeItem, TCategory>(ILogger<TCategoryTimerService> logger,
 IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
  IEnumerable<ITreeItemLoaderFactory<TTreeItem>> treeItemLoaderFactories,
   IEnumerable<ICategoryItemHandlerFactory<TCategory>> categoryItemHandlerFactories,
   IImportServiceLogFactory? logFactory = null)
    : CategoryServiceFactory<TCategoryTimerService, TTreeItem, TCategory>(logger, loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlerFactories, logFactory)
where TTreeItem : TreeItem.TreeItem, new()
where TCategoryTimerService : CategoryTimerService<TTreeItem, TCategory>
where TCategory : ICategory, new()
{
    private const string DefaultInterval = "DefaultInterval";

    public override Type GetServiceType()
    {
        return typeof(CategoryTimerServiceImpl<TTreeItem, TCategory>);
    }

    protected override TCategoryTimerService Create(ILogger<TCategoryTimerService> logger,
    string name,
    ILoaderService loader,
    string categorySourceUrl,
    string sourceName,
     string url,
      IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
      ICategoryItemHandler<TCategory> categoryItemHandler,
      IImportSettingsProvider importSettingsProvider,
      IImportSettings importSettings,
       object? categoryLoadData = null)
    {
        int? interval = importSettingsProvider.TryGet<int>(importSettings, DefaultInterval, out var configuredInterval) && configuredInterval > 0
            ? configuredInterval
            : null;

        return Create(logger, name, loader, categorySourceUrl, sourceName, url
               , categoryLoadStages, categoryItemHandler, importSettingsProvider, importSettings, categoryLoadData, interval);
    }

    protected abstract TCategoryTimerService Create(ILogger<TCategoryTimerService> logger,
    string name,
    ILoaderService loader,
    string categorySourceUrl,
    string sourceName,
     string url,
      IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
      ICategoryItemHandler<TCategory> categoryItemHandler,
      IImportSettingsProvider importSettingsProvider,
      IImportSettings importSettings,
       object? categoryLoadData = null,
       int? interval = null);
}