using Import.Catalog.Category.Interfaces;
using Import.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;
using Cat = global::Import.Catalog.Category.Interfaces.Category;

namespace Import.Catalog.Category.Service.Test;

public class CategoryTimerServiceTest(ILogger<CategoryTimerServiceTest> logger,
    string name,
    ILoaderService<LoadData<object>>? loader,
    ICategoryCatalogModel catalog,
    IEnumerable<ITreeItemLoader<TestCategory>> categoryLoadStages,
    ICategoryItemHandler<Cat> itemHandler,
    int timerSecondsInterval,
    object? loadData = null)
    : CategoryTimerService<object, TestCategory, Cat>(logger, 
    name, 
    loader, 
    catalog.CategorySourceUrl, 
    catalog.SourceName, 
    catalog.SourceUrl, 
    categoryLoadStages, 
    itemHandler,
     timerSecondsInterval,
    loadData)
{
}