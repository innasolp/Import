using Import.Catalog.Category.Interfaces;
using Import.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

internal class CategoryTimerServiceImpl<TTreeItem, TCategory>(ILogger<CategoryTimerServiceImpl<TTreeItem, TCategory>> logger,
string name,
ILoaderService loader,
string categorySourceUrl,
string sourceName,
string url,
IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
ICategoryItemHandler<TCategory> itemHandler,
int? timerSecondsInterval = null,
object? categoryLoadData = null)
: CategoryTimerService<TTreeItem, TCategory>(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadStages, itemHandler, timerSecondsInterval, categoryLoadData)
where TTreeItem : TreeItem.TreeItem, new()
where TCategory : ICategory, new()
{

}

internal class CategoryTimerServiceImpl(ILogger<CategoryTimerServiceImpl> logger,
string name,
ILoaderService loader,
string categorySourceUrl,
string sourceName,
string url,
IEnumerable<ITreeItemLoader<CategoryTreeItem>> categoryLoadStages,
ICategoryItemHandler<Interfaces.Category> itemHandler,
int? timerSecondsInterval = null,
object? categoryLoadData = null)
: CategoryTimerServiceImpl<CategoryTreeItem, Interfaces.Category>(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadStages, itemHandler, timerSecondsInterval, categoryLoadData)
{

}