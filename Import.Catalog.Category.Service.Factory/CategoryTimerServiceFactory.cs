using Import.Catalog.Category.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

public class CategoryTimerServiceFactory<TTreeItem>(ILogger logger,
 IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
  IEnumerable<ITreeItemLoaderFactory<TTreeItem>> treeItemLoaderFactories,
   IEnumerable<ICategoryItemHandler<TTreeItem>> categoryItemHandlers,
   IImportServiceLogFactory? logFactory = null)
    : CategoryServiceFactory<TTreeItem>(logger, loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlers, logFactory)
where TTreeItem : TreeItem.TreeItem, new()
{
    public override Type GetServiceType()
    {
        return typeof(CategoryTimerService<TTreeItem>);
    }


    protected override CategoryService<TTreeItem> Create(ILogger logger,
    string name,
    ILoaderService loader,
    string categorySourceUrl,
    string sourceName,
     string url,
      IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
      ICategoryItemHandler<TTreeItem> categoryItemHandler,
      IImportSettings importSettings,
       object? categoryLoadData = null)
    {
        int? interval = importSettings.TryGet<int>("DefaultInterval", out var configuredInterval) && configuredInterval > 0
            ? configuredInterval
            : null;

        return new CategoryTimerService<TTreeItem>(logger, name, loader, categorySourceUrl, sourceName, url
               , categoryLoadStages, categoryItemHandler, interval, categoryLoadData);
    }
}