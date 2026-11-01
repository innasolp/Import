using Import.Catalog.Category.Factory.Interfaces;
using Import.Catalog.Category.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

public abstract class CategoryTimerServiceFactory<TCategoryTimerService, TTreeItem>(ILogger<TCategoryTimerService> logger,
 IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
  IEnumerable<ITreeItemLoaderFactory<TTreeItem>> treeItemLoaderFactories,
   IEnumerable<ICategoryItemHandlerFactory<TTreeItem>> categoryItemHandlerFactories,
   IImportServiceLogFactory? logFactory = null)
    : CategoryServiceFactory<TCategoryTimerService, TTreeItem>(logger, loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlerFactories, logFactory)
where TTreeItem : TreeItem.TreeItem, new()
where TCategoryTimerService : CategoryTimerService<TTreeItem>
{
    private const string DefaultInterval = "DefaultInterval";

    public override Type GetServiceType()
    {
        return typeof(CategoryTimerService<TTreeItem>);
    }

    protected override TCategoryTimerService Create(ILogger<TCategoryTimerService> logger,
    string name,
    ILoaderService loader,
    string categorySourceUrl,
    string sourceName,
     string url,
      IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
      ICategoryItemHandler<TTreeItem> categoryItemHandler,
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
      ICategoryItemHandler<TTreeItem> categoryItemHandler,
      IImportSettingsProvider importSettingsProvider,
      IImportSettings importSettings,
       object? categoryLoadData = null,
       int? interval = null);
}