using Import.Catalog.Category.Interfaces;
using Import.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

internal class CategoryTimerServiceImpl<TTreeItem>(ILogger<CategoryTimerServiceImpl<TTreeItem>> logger,
string name,
ILoaderService loader,
string categorySourceUrl,
string sourceName,
string url,
IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
ICategoryItemHandler<TTreeItem> itemHandler,
int? timerSecondsInterval = null,
object? categoryLoadData = null)
: CategoryTimerService<TTreeItem>(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadStages, itemHandler, timerSecondsInterval, categoryLoadData)
where TTreeItem : TreeItem.TreeItem, new()
{

}

internal class CategoryTimerServiceImpl(ILogger<CategoryTimerServiceImpl> logger,
string name,
ILoaderService loader,
string categorySourceUrl,
string sourceName,
string url,
IEnumerable<ITreeItemLoader<Category>> categoryLoadStages,
ICategoryItemHandler<Category> itemHandler,
int? timerSecondsInterval = null,
object? categoryLoadData = null)
: CategoryTimerServiceImpl<Category>(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadStages, itemHandler, timerSecondsInterval, categoryLoadData)
{

}