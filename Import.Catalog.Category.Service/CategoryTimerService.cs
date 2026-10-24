using Import.Catalog.Category.Interfaces;
using Import.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service;

public class CategoryTimerService<TCategory> : CategoryService<TCategory>
   where TCategory : ITreeItem
{
    private readonly int _defaultInterval = 3600;

    private readonly PeriodicTimer _timer;

    public CategoryTimerService(ILogger logger,
                                string name,
                                ILoaderService loader,
                                string categorySourceUrl,
                                string sourceName,
                                string url,
                                IEnumerable<ITreeItemLoader<TCategory>> categoryLoadStages,
                                ICategoryItemHandler<TCategory> itemHandler,
                                int? timerSecondsInterval = null,
     object? categoryLoadData = null) : base(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadStages, itemHandler, categoryLoadData)
    {
        _timer = new(TimeSpan.FromSeconds(timerSecondsInterval ?? _defaultInterval));
    }

    protected override void Dispose()
    {
        _timer.Dispose();
        base.Dispose();
    }

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        await base.ProcessAsync(stoppingToken);

        var categoryLoadData = GetLoadData();
        while ((await _timer.WaitForNextTickAsync(stoppingToken).AsTask()) && !stoppingToken.IsCancellationRequested)
        {
            await LoadCategoriesAsync(categoryLoadData, stoppingToken);
        }
    }
}