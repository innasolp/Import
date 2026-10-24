using Import.Catalog.Category.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

public class CategoryServiceFactoryImpl<TTreeItem>(ILogger logger,
IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
IEnumerable<ITreeItemLoaderFactory<TTreeItem>> treeItemLoaderFactories,
 IEnumerable<ICategoryItemHandler<TTreeItem>> categoryItemHandlers,
  IImportServiceLogFactory? logFactory = null)
  : CategoryServiceFactory<TTreeItem>(logger, loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlers, logFactory)
   where TTreeItem : TreeItem.TreeItem, new()
{
    public override Type GetServiceType()
    {
        return typeof(CategoryService<TTreeItem>);
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
        return new CategoryServiceImpl<TTreeItem>(logger, name, loader, categorySourceUrl, sourceName, url
               , categoryLoadStages, categoryItemHandler, categoryLoadData);
    }
}