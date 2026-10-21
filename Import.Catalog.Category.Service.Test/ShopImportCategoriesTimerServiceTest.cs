using Import.Catalog.Category.Interfaces;
using Import.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Test;

public class CategoryTimerServiceTest(ILogger<CategoryTimerServiceTest> logger,
    string name,
    ILoaderService loader,
    ICategoryShopModel shop,
    IEnumerable<ITreeItemLoader<TestCategory>> categoryLoadStages,
    int timerSecondsInterval,
    ICategoryItemHandler<TestCategory> itemHandler)
    : CategoryService<TestCategory>(logger, name, loader, shop.CategorySourceUrl, shop.SourceName, shop.SourceUrl, categoryLoadStages, itemHandler)//, timerSecondsInterval)
{
}