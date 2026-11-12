using Import.Catalog.Category.Interfaces;
using Import.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

internal class CategoryServiceImpl<TTreeItem, TCategory>(ILogger<CategoryServiceImpl<TTreeItem, TCategory>> logger,
string name,
 ILoaderService loader,
  string categorySourceUrl,
  string sourceName,
   string url,
   IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
   ICategoryItemHandler<TCategory> itemHandler,
   object? categoryLoadData = null) : CategoryService<TTreeItem, TCategory>(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadStages, itemHandler, categoryLoadData)
   where TTreeItem : TreeItem.TreeItem, new()
   where TCategory : ICategory, new()
{
}

internal class CategoryServiceImpl(ILogger<CategoryServiceImpl> logger,
string name,
 ILoaderService loader,
  string categorySourceUrl,
  string sourceName,
   string url,
   IEnumerable<ITreeItemLoader<CategoryTreeItem>> categoryLoadStages,
   ICategoryItemHandler<Interfaces.Category> itemHandler,
   object? categoryLoadData = null) :
   CategoryServiceImpl<CategoryTreeItem, Interfaces.Category>(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadStages, itemHandler, categoryLoadData)
{
}