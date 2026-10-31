using Import.Catalog.Category.Factory.Interfaces;
using Import.Catalog.Category.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

internal class CategoryServiceFactoryImpl<TTreeItem>(ILogger<CategoryServiceImpl<TTreeItem>> logger,
IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
IEnumerable<ITreeItemLoaderFactory<TTreeItem>> treeItemLoaderFactories,
 IEnumerable<ICategoryItemHandlerFactory<TTreeItem>> categoryItemHandlerFactories,
  IImportServiceLogFactory? logFactory = null)
  : CategoryServiceFactory<CategoryServiceImpl<TTreeItem>, TTreeItem>(logger, loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlerFactories, logFactory)
   where TTreeItem : TreeItem.TreeItem, new()
{
    public override Type GetServiceType()
    {
        return typeof(CategoryServiceImpl);
    }

    protected override CategoryServiceImpl<TTreeItem> Create(ILogger<CategoryServiceImpl<TTreeItem>> logger,
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
        return new CategoryServiceImpl<TTreeItem>(logger, name, loader, categorySourceUrl, sourceName, url
                , categoryLoadStages, categoryItemHandler, categoryLoadData);
    }
}

internal class CategoryServiceFactoryImpl(ILogger<CategoryServiceImpl> logger,
IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
IEnumerable<ITreeItemLoaderFactory<Category>> treeItemLoaderFactories,
 IEnumerable<ICategoryItemHandlerFactory<Category>> categoryItemHandlerFactories,
  IImportServiceLogFactory? logFactory = null)
  : CategoryServiceFactory<CategoryServiceImpl, Category>(logger, loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlerFactories, logFactory)
{
    public override Type GetServiceType()
    {
        return typeof(CategoryServiceImpl);
    }

    protected override CategoryServiceImpl Create(ILogger<CategoryServiceImpl> logger,
     string name,
       ILoaderService loader,
       string categorySourceUrl,
        string sourceName,
        string url,
        IEnumerable<ITreeItemLoader<Category>> categoryLoadStages,
        ICategoryItemHandler<Category> categoryItemHandler,
        IImportSettingsProvider importSettingsProvider,
        IImportSettings importSettings,
        object? categoryLoadData = null)
    {
        return new CategoryServiceImpl(logger, name, loader, categorySourceUrl, sourceName, url
               , categoryLoadStages, categoryItemHandler, categoryLoadData);
    }
}