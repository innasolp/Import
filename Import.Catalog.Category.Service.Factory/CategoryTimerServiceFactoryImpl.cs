using Import.Catalog.Category.Factory.Interfaces;
using Import.Catalog.Category.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

internal class CategoryTimerServiceFactoryImpl<TTreeItem, TCategory>
(ILogger<CategoryTimerServiceImpl<TTreeItem, TCategory>> logger,
IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
 IEnumerable<ITreeItemLoaderFactory<TTreeItem>> treeItemLoaderFactories,
  IEnumerable<ICategoryItemHandlerFactory<TCategory>> categoryItemHandlerFactories,
   IImportServiceLogFactory? logFactory = null)
    : CategoryTimerServiceFactory<CategoryTimerServiceImpl<TTreeItem, TCategory>, TTreeItem, TCategory>(logger, loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlerFactories, logFactory)
where TTreeItem : TreeItem.TreeItem, new()
where TCategory : ICategory, new()
{

    protected override CategoryTimerServiceImpl<TTreeItem, TCategory> Create(ILogger<CategoryTimerServiceImpl<TTreeItem, TCategory>> logger,
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
      int? interval = null)
    {
        return new CategoryTimerServiceImpl<TTreeItem, TCategory>(logger, name, loader, categorySourceUrl, sourceName, url
               , categoryLoadStages, categoryItemHandler, interval, categoryLoadData);
    }

    public override Type GetServiceType()
    {
        return typeof(CategoryTimerServiceImpl<TTreeItem, TCategory>);
    }
}

internal class CategoryTimerServiceFactoryImpl(ILogger<CategoryTimerServiceImpl> logger,
IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
 IEnumerable<ITreeItemLoaderFactory<CategoryTreeItem>> treeItemLoaderFactories,
  IEnumerable<ICategoryItemHandlerFactory<Interfaces.Category>> categoryItemHandlerFactories,
   IImportServiceLogFactory? logFactory = null)
   : CategoryTimerServiceFactory<CategoryTimerServiceImpl, CategoryTreeItem, Interfaces.Category>(logger, loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlerFactories, logFactory)
{

    protected override CategoryTimerServiceImpl Create(ILogger<CategoryTimerServiceImpl> logger,
    string name,
    ILoaderService loader,
    string categorySourceUrl,
    string sourceName,
    string url,
    IEnumerable<ITreeItemLoader<CategoryTreeItem>> categoryLoadStages,
     ICategoryItemHandler<Interfaces.Category> categoryItemHandler,
     IImportSettingsProvider importSettingsProvider,
     IImportSettings importSettings,
     object? categoryLoadData = null,
      int? interval = null)
    {
        return new CategoryTimerServiceImpl(logger, name, loader, categorySourceUrl, sourceName, url
               , categoryLoadStages, categoryItemHandler, interval, categoryLoadData);
    }

    public override Type GetServiceType()
    {
        return typeof(CategoryTimerServiceImpl);
    }
}