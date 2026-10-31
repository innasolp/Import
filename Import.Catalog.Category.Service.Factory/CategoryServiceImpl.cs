using Import.Catalog.Category.Interfaces;
using Import.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

internal class CategoryServiceImpl<TTreeItem>(ILogger<CategoryServiceImpl<TTreeItem>> logger,
string name,
 ILoaderService loader,
  string categorySourceUrl,
  string sourceName,
   string url,
   IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
   ICategoryItemHandler<TTreeItem> itemHandler,
   object? categoryLoadData = null) : CategoryService<TTreeItem>(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadStages, itemHandler, categoryLoadData)
   where TTreeItem : TreeItem.TreeItem, new()
{
}

internal class CategoryServiceImpl(ILogger<CategoryServiceImpl> logger,
string name,
 ILoaderService loader,
  string categorySourceUrl,
  string sourceName,
   string url,
   IEnumerable<ITreeItemLoader<Category>> categoryLoadStages,
   ICategoryItemHandler<Category> itemHandler,
   object? categoryLoadData = null) : CategoryServiceImpl<Category>(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadStages, itemHandler, categoryLoadData)
{
}