using Import.Catalog.Product.Interfaces;
using Import.Interfaces;
using Import.Service;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Import.Catalog.Product.Service;

public abstract class ProductService<TCategory, TProductItem>(
    ILogger logger,
    ILoaderService loader,
    string url,
    IEnumerable<IProductCatalogCategory> catalogCategories,
    IProductItemHandler itemHandler,
    string productPathFormat,
    string categoryPathFormat,
    string sourceName,
    ICategoryPaging<TCategory> categoryPaging,
    IItemSerializer<TCategory> categorySerializer,
    ProductServiceOptions importProductServiceOptions)
    : ImportService(logger, loader, url), IListener<IProductCatalogCategory>
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    public enum AttemptResult
    {
        AttemptsReachedTheMaximum,
        AttemptsAvailable,
        Success
    }

    protected sealed record ImportProduct(
        IProductItem ProductItem,
        string SourceName,
        string SourcePath,
        Stream Stream) : IImportProduct;

    protected readonly IProductItemHandler ItemHandler = itemHandler;

    protected ConcurrentQueue<IProductCatalogCategory> Categories { get; } = new(catalogCategories);

    private readonly ConcurrentQueue<IProductCatalogCategory> _failedCategories = new();

    protected readonly string ProductPathFormat = productPathFormat;
    protected readonly string CategoryPathFormat = categoryPathFormat; // Исправлена кириллическая 'С'
    protected readonly string SourceName = sourceName;

    private readonly ICategoryPaging<TCategory> _categoryPaging = categoryPaging;
    private readonly IItemSerializer<TCategory> _categorySerializer = categorySerializer;

    protected virtual int MaxUnsuccessRequestCount => 10;
    protected virtual int ConcurrentCategoryTaskCount => 10;

    protected ProductServiceOptions ImportProductServiceOptions { get; } = importProductServiceOptions;

    Task IListener<IProductCatalogCategory>.On(IProductCatalogCategory message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Categories.Enqueue(message);
        Logger.LogInformation(ImportProductLogMessages.NewCategoryIsEnqueued, message.Path);
        return Task.CompletedTask;
    }

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        await ProcessCategories(stoppingToken).ConfigureAwait(false);
    }

    protected virtual async Task ProcessCategories(CancellationToken stoppingToken)
    {
        while (true)
        {
            stoppingToken.ThrowIfCancellationRequested();

            while (_failedCategories.TryDequeue(out var failedCategory))
            {
                Categories.Enqueue(failedCategory);
            }

            var concurrentCategories = new List<IProductCatalogCategory>(ConcurrentCategoryTaskCount);

            while (concurrentCategories.Count < ConcurrentCategoryTaskCount &&
                   Categories.TryDequeue(out var productCatalogCategory))
            {
                if (productCatalogCategory != null)
                    concurrentCategories.Add(productCatalogCategory);
            }

            if (concurrentCategories.Count == 0)
            {
                await Task.Delay(50, stoppingToken).ConfigureAwait(false);
                continue;
            }

            await Task.WhenAll(concurrentCategories.Select(c => ProcessCategoryAsync(c, stoppingToken)))
                .ConfigureAwait(false);
        }
    }

    protected virtual Task ProcessCategoryAsync(IProductCatalogCategory category, CancellationToken stoppingToken)
    {
        var categoryState = CategoryProcessState.Start(category.Path);
        return ProcessCategoryAsync(category, categoryState, stoppingToken);
    }

    protected async Task ProcessCategoryAsync(
        IProductCatalogCategory category,
        CategoryProcessState categoryState,
        CancellationToken stoppingToken)
    {
        bool? isEndOfCategory = null;

        if (string.IsNullOrEmpty(categoryState.CategoryPagePath))
        {
            categoryState.CategoryPagePath = _categoryPaging.GetCategoryPagePath(
                category,
                CategoryPathFormat,
                ImportProductServiceOptions.CategoryPathFormatType,
                categoryState.Page);
        }

        while (isEndOfCategory != true)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var (success, categoryResult, successCount) = await TryProcessCategoryPageAsync(
                categoryState.CategoryPagePath,
                category.ItemId,
                category.Path,
                categoryState.Page,
                stoppingToken).ConfigureAwait(false);

            var attemptResult = ApplyCategoryStateAttempts(category, categoryState, success);

            if (attemptResult == AttemptResult.AttemptsReachedTheMaximum)
                break;

            if (attemptResult == AttemptResult.AttemptsAvailable)
            {
                var retryDelay = GetRetryDelay(categoryState.AttemptsCount);
                await Task.Delay(retryDelay, stoppingToken).ConfigureAwait(false);
                continue;
            }

            if (successCount == null)
            {
                var resolvedPagePath = _categoryPaging.GetCategoryPagePath(
                    category,
                    CategoryPathFormat,
                    ImportProductServiceOptions.CategoryPathFormatType,
                    categoryState.Page,
                    categoryResult);

                if (string.IsNullOrEmpty(resolvedPagePath) || resolvedPagePath == categoryState.CategoryPagePath)
                    break;

                categoryState.CategoryPagePath = resolvedPagePath;
                continue;
            }

            categoryState.CategoryPagePath = _categoryPaging.GetNextCategoryPagePath(
                category,
                CategoryPathFormat,
                ImportProductServiceOptions.CategoryPathFormatType,
                categoryState.Page,
                categoryResult);

            categoryState.ApplyPageIterationResult(successCount, categoryResult?.CategoryProductItems.Length);

            isEndOfCategory = categoryResult != null &&
                (IsLastPage(categoryResult, categoryState.Page) == true ||
                 IsEndOfCategory(categoryResult, categoryState.AllProductCount) == true);

            categoryState.IncrementPage();

            await OnPageProcessedAsync(categoryResult, stoppingToken).ConfigureAwait(false);
        }

        Logger.LogInformation(
            ImportProductLogMessages.CategoryCompletedInfo,
            category.Category,
            categoryState.SuccessProductCount,
            categoryState.UnsuccessProductCount);
    }

    protected AttemptResult ApplyCategoryStateAttempts(
        IProductCatalogCategory category,
        CategoryProcessState categoryState,
        bool success)
    {
        if (!success)
        {
            categoryState.IncrementAttempts();

            if (categoryState.AttemptsCount >= MaxUnsuccessRequestCount)
            {
                Logger.LogError(
                    ImportProductLogMessages.FailedToLoadCategoryPageAfterAttempts,
                    category.Path,
                    categoryState.Page,
                    MaxUnsuccessRequestCount);
                _failedCategories.Enqueue(category);
                return AttemptResult.AttemptsReachedTheMaximum;
            }

            return AttemptResult.AttemptsAvailable;
        }

        categoryState.ResetAttempts();
        return AttemptResult.Success;
    }

    protected virtual TimeSpan GetRetryDelay(int attemptsCount)
    {
        var delayMs = Math.Min(
            (int)Math.Pow(2, attemptsCount) * 100,
            MaxUnsuccessRequestCount * 1000);
        return TimeSpan.FromMilliseconds(delayMs);
    }

    protected bool? IsLastPage(TCategory category, int page)
    {
        var totalCount = category.TotalCount;
        if (totalCount > 0 && ImportProductServiceOptions.PageProductCount > 0)
        {
            var totalPageCount = (totalCount + ImportProductServiceOptions.PageProductCount - 1)
                / ImportProductServiceOptions.PageProductCount;

            return page >= totalPageCount;
        }

        return null;
    }

    protected virtual async Task<(bool success, TCategory?, int? successCount)> TryProcessCategoryPageAsync(
        string categoryPagePath,
        int categoryItemId,
        string categoryPath,
        int page,
        CancellationToken stoppingToken)
    {
        var loaderData = GetLoaderData();
        var requestData = ImportProductServiceOptions.CategoryLoadData != null
            ? new object?[] { loaderData, ImportProductServiceOptions.CategoryLoadData, new string[] { $"{categoryItemId}", $"{page}" } }
            : loaderData;

        var (success, category) = await TryGetCategoryFromPathAsync(
            categoryPagePath,
            requestData,
            categoryPath,
            page,
            stoppingToken).ConfigureAwait(false);

        if (!success)
        {
            Logger.LogInformation(ImportProductLogMessages.CategoryNotLoadedFromUrl, categoryPagePath);
            return (false, default, default);
        }

        if (category?.CategoryProductItems is not { Length: > 0 })
            return (true, category, default);

        int successCount = await ProcessCategoryProductItems(categoryItemId, category, stoppingToken).ConfigureAwait(false);

        if (successCount == category.CategoryProductItems.Length)
            Logger.LogInformation(ImportProductLogMessages.CategoryProductsForUrlWereProcessedSuccesfullInfo, categoryPagePath, successCount);
        else if (successCount == 0)
            Logger.LogInformation(ImportProductLogMessages.CategoryProductsWereNotLoadedError, categoryPagePath);
        else
            Logger.LogWarning(ImportProductLogMessages.NotAllCategoryProductsForUrlWereProcessedWarning, categoryPath, successCount, categoryPagePath);

        return (true, category, successCount);
    }

    protected virtual async Task<int> ProcessCategoryProductItems(
        int categoryItemId,
        TCategory category,
        CancellationToken stoppingToken)
    {
        var successCount = 0;
        foreach (var categoryProductItem in category.CategoryProductItems)
        {
            stoppingToken.ThrowIfCancellationRequested();
            categoryProductItem.CategoryItemId = categoryItemId;

            if (await TryProcessCategoryProductAsync(categoryProductItem, stoppingToken).ConfigureAwait(false))
                successCount++;
        }

        return successCount;
    }

    protected virtual async Task<bool> TryProcessCategoryProductAsync(
        ICategoryProductItem categoryProductItem,
        CancellationToken cancellationToken)
    {
        var path = GetProductAbsolutePath(ProductPathFormat, categoryProductItem);
        var loaderData = GetLoaderData();

        var (success, stream) = await TryLoadProductItemAsync(categoryProductItem, path, loaderData, cancellationToken)
            .ConfigureAwait(false);

        if (!success || stream is null)
        {
            if (stream is not null)
                await stream.DisposeAsync().ConfigureAwait(false);

            Logger.LogInformation(ImportProductLogMessages.ProductWasNotLoadedFromUrlWithError, path);
            return false;
        }

        await using (stream.ConfigureAwait(false))
        {
            var productItem = CreateProductItemFromCategoryProductItem(categoryProductItem, path);
            Logger.LogInformation(ImportProductLogMessages.ProductHasBeenSuccessfullyLoadedFromUrl, productItem.Name, path);

            return await TryHandleProduct(productItem, stream, cancellationToken).ConfigureAwait(false);
        }
    }

    protected async Task<(bool success, Stream? stream)> TryLoadProductItemAsync(
        ICategoryProductItem categoryProductItem,
        string path,
        object? loaderData,
        CancellationToken cancellationToken)
    {
        try
        {
            var requestData = ImportProductServiceOptions.ProductLoadData is not null
                ? new object?[] { loaderData, ImportProductServiceOptions.ProductLoadData, new string[] { $"{categoryProductItem.Id}" } }
                : loaderData;

            return await TryLoadFromUrlAsync(path, requestData, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            Logger.LogInformation(ImportProductLogMessages.FailedToLoadProductOfCategory, path, categoryProductItem.CategoryItemId);
            throw;
        }
    }

    protected async Task<bool> TryHandleProduct(
        TProductItem productItem,
        Stream stream,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await ItemHandler.HandleItem(
                new ImportProduct(productItem, SourceName, Host, stream),
                cancellationToken).ConfigureAwait(false);

            Logger.LogInformation(
                ImportProductLogMessages.ProductFromUrlHandledWithStatusInfo,
                productItem.Name,
                productItem.AbsolutePath,
                result);
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            Logger.LogError(e, ImportProductLogMessages.ProductFromUrlHandlingFailed, productItem.AbsolutePath);
            return false;
        }
    }

    protected virtual async Task<(bool success, TCategory? result)> TryGetCategoryFromPathAsync(
        string dataPath,
        object? requestData,
        string categoryPath,
        int page,
        CancellationToken token)
    {
        bool success;
        Stream? stream;
        try
        {
            (success, stream) = await TryLoadFromUrlAsync(dataPath, requestData, cancellationToken: token)
                .ConfigureAwait(false);
        }
        catch
        {
            Logger.LogInformation(ImportProductLogMessages.FailedToLoadCategoryPage, categoryPath, page);
            throw;
        }

        if (!success || stream is null)
        {
            if (stream is not null)
                await stream.DisposeAsync().ConfigureAwait(false);

            return (false, default);
        }

        await using (stream.ConfigureAwait(false))
        {
            Logger.LogInformation(ImportProductLogMessages.CategoryPageLoadedSuccessfully, categoryPath, page);

            try
            {
                var item = await _categorySerializer.DeserializeFromStream(stream, cancellationToken: token)
                    .ConfigureAwait(false);
                return (true, item);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) // Ловим любые ошибки десериализации, а не только JsonException
            {
                Logger.LogError(ex, ImportProductLogMessages.SerializationFailed, typeof(TCategory).Name, dataPath);
                return (false, default);
            }
        }
    }

    protected TProductItem CreateProductItemFromCategoryProductItem(
        ICategoryProductItem categoryProductItem,
        string productPath)
    {
        return new TProductItem
        {
            Path = categoryProductItem.ItemPath,
            ItemId = categoryProductItem.Id,
            Name = categoryProductItem.Name,
            Price = categoryProductItem.Price,
            Currency = categoryProductItem.Currency,
            AbsolutePath = productPath,
            CategoryId = categoryProductItem.CategoryItemId,
            Brand = categoryProductItem.Brand
        };
    }

    protected virtual bool? IsEndOfCategory(TCategory category, int processProductCount)
    {
        return category.TotalCount > 0
            ? category.TotalCount <= processProductCount
            : null;
    }

    protected virtual string GetProductAbsolutePath(string productPathFormat, ICategoryProductItem productItem)
    {
        return ImportProductServiceOptions.ProductPathFormatType switch
        {
            PathFormatType.ItemId => string.Format(productPathFormat, productItem.Id),
            PathFormatType.Path => string.Format(productPathFormat, PreparePath(productItem.ItemPath)),
            PathFormatType.CategoryWithItemId => string.Format(productPathFormat, productItem.Id, PreparePath(productItem.ItemPath)),
            _ => string.Format(productPathFormat, productItem.ItemPath),
        };
    }

    protected virtual Task OnPageProcessedAsync(TCategory? category, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected abstract string PreparePath(string path);
}