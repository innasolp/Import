using Import.Catalog.Category.Factory.Interfaces;
using Import.Catalog.Category.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

internal class CategoryTimerServiceFactoryImpl<TTreeItem>(ILogger<CategoryTimerServiceImpl<TTreeItem>> logger,
IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
 IEnumerable<ITreeItemLoaderFactory<TTreeItem>> treeItemLoaderFactories,
  IEnumerable<ICategoryItemHandlerFactory<TTreeItem>> categoryItemHandlerFactories,
   IImportServiceLogFactory? logFactory = null)
    : CategoryTimerServiceFactory<CategoryTimerServiceImpl<TTreeItem>, TTreeItem>(logger, loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlerFactories, logFactory)
where TTreeItem : TreeItem.TreeItem, new()
{

    protected override CategoryTimerServiceImpl<TTreeItem> Create(ILogger<CategoryTimerServiceImpl<TTreeItem>> logger,
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
      int? interval = null)
    {
        return new CategoryTimerServiceImpl<TTreeItem>(logger, name, loader, categorySourceUrl, sourceName, url
               , categoryLoadStages, categoryItemHandler, interval, categoryLoadData);
    }

    public override Type GetServiceType()
    {
        return typeof(CategoryTimerServiceImpl<TTreeItem>);
    }
}

internal class CategoryTimerServiceFactoryImpl(ILogger<CategoryTimerServiceImpl> logger,
IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
 IEnumerable<ITreeItemLoaderFactory<Category>> treeItemLoaderFactories,
  IEnumerable<ICategoryItemHandlerFactory<Category>> categoryItemHandlerFactories,
   IImportServiceLogFactory? logFactory = null)
   : CategoryTimerServiceFactory<CategoryTimerServiceImpl, Category>(logger, loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlerFactories, logFactory)
{

    protected override CategoryTimerServiceImpl Create(ILogger<CategoryTimerServiceImpl> logger,
    string name,
    ILoaderService loader,
    string categorySourceUrl,
    string sourceName,
    string url,
    IEnumerable<ITreeItemLoader<Category>> categoryLoadStages,
     ICategoryItemHandler<Category> categoryItemHandler,
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