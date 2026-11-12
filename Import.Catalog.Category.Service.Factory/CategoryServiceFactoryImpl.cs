using Import.Catalog.Category.Factory.Interfaces;
using Import.Catalog.Category.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

internal class CategoryServiceFactoryImpl<TTreeItem, TCategory>(ILogger<CategoryServiceImpl<TTreeItem, TCategory>> logger,
IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
IEnumerable<ITreeItemLoaderFactory<TTreeItem>> treeItemLoaderFactories,
 IEnumerable<ICategoryItemHandlerFactory<TCategory>> categoryItemHandlerFactories,
  IImportServiceLogFactory? logFactory = null)
  : CategoryServiceFactory<CategoryServiceImpl<TTreeItem, TCategory>, TTreeItem, TCategory>(logger,
    loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlerFactories, logFactory)
   where TTreeItem : TreeItem.TreeItem, new()
   where TCategory : ICategory, new()
{
    public override Type GetServiceType()
    {
        return typeof(CategoryServiceImpl<TTreeItem, TCategory>);
    }

    protected override CategoryServiceImpl<TTreeItem, TCategory> Create(ILogger<CategoryServiceImpl<TTreeItem, TCategory>> logger,
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
        return new CategoryServiceImpl<TTreeItem, TCategory>(logger, name, loader, categorySourceUrl, sourceName, url
                , categoryLoadStages, categoryItemHandler, categoryLoadData);
    }
}

internal class CategoryServiceFactoryImpl(ILogger<CategoryServiceImpl> logger,
IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
IEnumerable<ITreeItemLoaderFactory<CategoryTreeItem>> treeItemLoaderFactories,
 IEnumerable<ICategoryItemHandlerFactory<Interfaces.Category>> categoryItemHandlerFactories,
  IImportServiceLogFactory? logFactory = null)
  : CategoryServiceFactory<CategoryServiceImpl, CategoryTreeItem, Interfaces.Category>(logger, loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlerFactories, logFactory)
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
        IEnumerable<ITreeItemLoader<CategoryTreeItem>> categoryLoadStages,
        ICategoryItemHandler<Interfaces.Category> categoryItemHandler,
        IImportSettingsProvider importSettingsProvider,
        IImportSettings importSettings,
        object? categoryLoadData = null)
    {
        return new CategoryServiceImpl(logger, name, loader, categorySourceUrl, sourceName, url
               , categoryLoadStages, categoryItemHandler, categoryLoadData);
    }
}