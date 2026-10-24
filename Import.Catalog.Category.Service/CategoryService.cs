using System.Collections.Concurrent;
using Import.Catalog.Category.Interfaces;
using Import.Interfaces;
using Import.Service;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service;

public abstract class CategoryService<TCategory> : ImportService
    where TCategory : ITreeItem
{
    private const int ConcurrencyDegree = 8;
    private readonly object _visitedIdsLock = new();

    protected sealed record ImportCategory(
        TCategory Category,
        string CategorySourceUrl,
        string SourceName,
        string SourceUrl) : IImportCategory<TCategory>;

    public override string Name { get; }

    private readonly string _categorySourceUrl;
    private readonly string _sourceName;
    private readonly ICategoryItemHandler<TCategory> _itemHandler;
    private readonly object? _categoryLoadData;
    private readonly IEnumerable<ITreeItemLoader<TCategory>> _categoryLoadStages;
    private readonly SemaphoreSlim _handleSemaphore = new(ConcurrencyDegree, ConcurrencyDegree);

    public CategoryService(
        ILogger logger,
        string name,
        ILoaderService loader,
        string categorySourceUrl,
        string sourceName,
        string url,
        IEnumerable<ITreeItemLoader<TCategory>> categoryLoadStages,
        ICategoryItemHandler<TCategory> itemHandler,
        object? categoryLoadData = null) : base(logger, loader, url)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        Name = name;

        ArgumentException.ThrowIfNullOrEmpty(categorySourceUrl, nameof(categorySourceUrl));
        _categorySourceUrl = categorySourceUrl;

        ArgumentException.ThrowIfNullOrEmpty(sourceName, nameof(sourceName));
        _sourceName = sourceName;

        ArgumentNullException.ThrowIfNull(itemHandler, nameof(itemHandler));
        _itemHandler = itemHandler;

        _categoryLoadData = categoryLoadData;

        if (!categoryLoadStages.Any())
            throw new ArgumentException("Collection cannot be empty.", nameof(categoryLoadStages));
        _categoryLoadStages = categoryLoadStages;
    }

    protected object? GetLoadData()
    {
        var loaderData = GetLoaderData();
        return loaderData is not null && _categoryLoadData is not null
            ? new object?[] { loaderData, _categoryLoadData }
            : loaderData ?? _categoryLoadData;
    }

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        var categoryLoadData = GetLoadData();
        await LoadCategoriesAsync(categoryLoadData, stoppingToken).ConfigureAwait(false);
    }

    protected async Task LoadCategoriesAsync(object? categoryLoadData, CancellationToken stoppingToken)
    {
        var nextStageParentCategories = new List<TCategory>();
        var parentCategories = new List<TCategory>();
        var visitedIds = new HashSet<int>();
        bool isFirstStage = true;

        try
        {
            foreach (var stage in _categoryLoadStages)
            {
                stoppingToken.ThrowIfCancellationRequested();

                if (isFirstStage)
                {
                    isFirstStage = false;

                    var (success, loadedCategories) = await LoadCategoryChildrenAsync(
                        _categorySourceUrl,
                        categoryLoadData,
                        stage,
                        parentCategory: default,
                        cancellationToken: stoppingToken).ConfigureAwait(false);

                    if (!success)
                    {
                        Logger.LogInformation(ImportCategoryLogMessages.CategoriesWereNotLoaded, _sourceName, Host);
                        return;
                    }

                    nextStageParentCategories.AddRange(loadedCategories.Where(c => c.Children.Count == 0));
                    parentCategories.AddRange(loadedCategories.Where(c => c.ParentId is null));

                    foreach (var category in loadedCategories)
                        visitedIds.Add(category.Id);

                    foreach (var category in loadedCategories)
                    {
                        Logger.LogInformation(
                            ImportCategoryLogMessages.CategoryNameIdForCatalogWasLoaded,
                            category.Name,
                            category.Id,
                            _sourceName);
                    }

                    await Task.WhenAll(loadedCategories.Select(c => HandleCategoryAsync(c, stoppingToken)))
                        .ConfigureAwait(false);
                }
                else
                {
                    if (nextStageParentCategories.Count == 0 || string.IsNullOrEmpty(stage.TreeItemLoadOptions.UrlFormat))
                        break;

                    var loadedCategories = await LoadCategoryChildrenAsync(
                        stage.TreeItemLoadOptions.UrlFormat,
                        stage,
                        nextStageParentCategories,
                        categoryLoadData,
                        stoppingToken,
                        visitedIds).ConfigureAwait(false);

                    foreach (var category in loadedCategories)
                        visitedIds.Add(category.Id);

                    await Task.WhenAll(loadedCategories.Select(c => HandleCategoryAsync(c, stoppingToken)))
                        .ConfigureAwait(false);

                    nextStageParentCategories = [.. loadedCategories.Where(c => c.Children.Count == 0)];
                }
            }
        }
        finally
        {
            foreach (var parentCategory in parentCategories.OfType<IDisposable>())
            {
                parentCategory.Dispose();
            }
        }
    }

    private async Task<IReadOnlyList<TCategory>> LoadCategoryChildrenAsync(
        string urlFormat,
        ITreeItemLoader<TCategory> stage,
        IEnumerable<TCategory> parentCategories,
        object? categoryLoadData,
        CancellationToken stoppingToken,
        HashSet<int> visitedIds)
    {
        var currentCategories = new ConcurrentBag<TCategory>();

        // Seed with parent IDs to avoid loading a leaf whose ID matches the requested parent
        foreach (var parentCategory in parentCategories)
            visitedIds.Add(parentCategory.Id);

        await Parallel.ForEachAsync(
            parentCategories,
            new ParallelOptions { CancellationToken = stoppingToken, MaxDegreeOfParallelism = ConcurrencyDegree },
            async (parentCategory, ct) =>
            {
                var url = string.Format(urlFormat, parentCategory.Id);
                var (success, loadedCategories) = await LoadCategoryChildrenAsync(
                    url,
                    categoryLoadData,
                    stage,
                    parentCategory,
                    required: !stage.IsTreeItem,
                    cancellationToken: ct).ConfigureAwait(false);

                if (success)
                {
                    foreach (var category in loadedCategories)
                    {
                        lock (_visitedIdsLock)
                        {
                            if (visitedIds.Add(category.Id))
                                currentCategories.Add(category);
                        }
                    }
                }
            }).ConfigureAwait(false);

        if (stage.IsTreeItem && !currentCategories.IsEmpty)
        {
            var loadedNextParentCategories = currentCategories.Where(c => c.Children.Count == 0).ToList();
            if (loadedNextParentCategories.Count > 0)
            {
                var loadedCategories = await LoadCategoryChildrenAsync(
                    urlFormat,
                    stage,
                    loadedNextParentCategories,
                    categoryLoadData,
                    stoppingToken,
                    visitedIds).ConfigureAwait(false);

                foreach (var category in loadedCategories)
                    currentCategories.Add(category);
            }
        }

        return [.. currentCategories];
    }

    private async Task<(bool success, IReadOnlyList<TCategory> result)> LoadCategoryChildrenAsync(
        string url,
        object? categoryLoadData,
        ITreeItemLoader<TCategory> stage,
        TCategory? parentCategory,
        bool required = true,
        CancellationToken cancellationToken = default)
    {
        var (success, stream) = await TryLoadFromUrlAsync(url, categoryLoadData, cancellationToken).ConfigureAwait(false);

        if (!success || stream is null)
        {
            if (required)
                Logger.LogInformation(ImportCategoryLogMessages.CategoryWasNotLoaded, url);

            return (false, []);
        }

        try
        {
            await using (stream.ConfigureAwait(false))
            {
                var loadedCategories = await stage.LoadAsync(parentCategory, stream, cancellationToken).ConfigureAwait(false);
                return (true, loadedCategories as IReadOnlyList<TCategory> ?? [.. loadedCategories]);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            Logger.LogError(e, ImportCategoryLogMessages.ChildrenForCategoryWereNotLoaded, url);
            return (false, []);
        }
    }

    private async Task HandleCategoryAsync(TCategory category, CancellationToken cancellationToken)
    {
        await _handleSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var resultStatus = await _itemHandler.HandleItem(
                new ImportCategory(category, _categorySourceUrl, _sourceName, Host),
                cancellationToken).ConfigureAwait(false);

            Logger.LogInformation(
                ImportCategoryLogMessages.CategoryNameIdForCatalogWasHandled,
                category.Name,
                category.Id,
                _sourceName,
                resultStatus);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ImportCategoryLogMessages.CategoryHandlingError, category.Name, category.Id, ex.Message);
            throw;
        }
        finally
        {
            _handleSemaphore.Release();
        }
    }

    protected override void Dispose()
    {
        _handleSemaphore.Dispose();
        base.Dispose();
    }
}