using Import.Catalog.Product.Interfaces;
using Import.Interfaces;
using Import.KeyHash;
using Import.ServiceState;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Stateful;

public abstract class ProductsStatefulService<TCategory, TProductItem>(
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
    ProductServiceOptions importProductServiceOptions,
    IKeyHasher keyHasher,
    IServiceStateRepository serviceStateRepository)
    : ProductService<TCategory, TProductItem>(
        logger,
        loader,
        url,
        catalogCategories,
        itemHandler,
        productPathFormat,
        categoryPathFormat,
        sourceName,
        categoryPaging,
        categorySerializer,
        importProductServiceOptions)
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    private readonly ServiceStateWorker<TCategory> _serviceStateWorker = new(serviceStateRepository);
    private bool _allCategoriesCompletedSuccessfully;
    private IProductCatalogCategory? _interruptedCategory;

    protected virtual object GetServiceStateKeyObject()
    {
        return new
        {
            Name,
            SourceName,
            Url = Host,
            ProductPathFormat,
            CategoryPathFormat,
            ItemHandlerType = ItemHandler.GetType().Name
        };
    }

    protected override async Task ProcessAsync(CancellationToken stoppingToken)
    {
        _allCategoriesCompletedSuccessfully = false;

        var serviceStateKeyObject = GetServiceStateKeyObject();
        var serviceStateKey = keyHasher.Hash(serviceStateKeyObject);

        await _serviceStateWorker.Start(serviceStateKey, stoppingToken).ConfigureAwait(false);
        await base.ProcessAsync(stoppingToken).ConfigureAwait(false);

        _allCategoriesCompletedSuccessfully = true;
    }

    protected override async Task ProcessCategories(CancellationToken stoppingToken)
    {
        if (_serviceStateWorker.ProductCatalogCategory is { } savedCategory)
        {
            _interruptedCategory = null;
            RemoveUpToAndIncludingCategory(savedCategory);
            Logger.LogInformation(ImportProductStatefulLogMessages.CategoryWasLoadedFromState, savedCategory.Path);
            await ProcessCategoryAsync(savedCategory, stoppingToken).ConfigureAwait(false);
        }
        else if (_interruptedCategory != null)
        {
            Categories.Enqueue(_interruptedCategory);
            _interruptedCategory = null;
        }

        while (!Categories.IsEmpty)
        {
            stoppingToken.ThrowIfCancellationRequested();

            if (!Categories.TryDequeue(out var category))
                continue;

            try
            {
                await ProcessCategoryAsync(category, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _interruptedCategory = category;
                throw;
            }
        }
    }

    private void RemoveUpToAndIncludingCategory(IProductCatalogCategory targetCategory)
    {
        bool existsInQueue = Categories.Any(c =>
            c.ItemId == targetCategory.ItemId &&
            c.Path == targetCategory.Path &&
            c.Category == targetCategory.Category);

        if (!existsInQueue)
            return;

        while (Categories.TryDequeue(out var current))
        {
            if (current.ItemId == targetCategory.ItemId &&
                current.Path == targetCategory.Path &&
                current.Category == targetCategory.Category)
            {
                // Саму целевую категорию тоже извлекаем, чтобы не обработать её повторно на следующей итерации
                break;
            }
        }
    }

    protected override async Task ProcessCategoryAsync(IProductCatalogCategory category, CancellationToken stoppingToken)
    {
        await _serviceStateWorker.SetProductCatalogCategoryIfNeedAsync(category, stoppingToken).ConfigureAwait(false);

        var categoryState = _serviceStateWorker.CategoryProcessState;
        await ProcessCategoryAsync(category, categoryState, stoppingToken).ConfigureAwait(false);

        if (categoryState.AttemptsCount < MaxUnsuccessRequestCount)
        {
            await _serviceStateWorker.ResetAsync(stoppingToken).ConfigureAwait(false);
        }
    }

    protected override async Task<(bool success, TCategory?, int? successCount)> TryProcessCategoryPageAsync(
        string categoryPagePath,
        int categoryItemId,
        string categoryPath,
        int page,
        CancellationToken stoppingToken)
    {
        return await base.TryProcessCategoryPageAsync(
            categoryPagePath,
            categoryItemId,
            categoryPath,
            page,
            stoppingToken).ConfigureAwait(false);
    }

    protected override Task OnPageProcessedAsync(TCategory? category, CancellationToken cancellationToken)
    {
        return _serviceStateWorker.CompleteCategoryPageAsync(cancellationToken);
    }

    protected override async Task<(bool success, TCategory? result)> TryGetCategoryFromPathAsync(
        string dataPath,
        object? requestData,
        string categoryPath,
        int page,
        CancellationToken token)
    {
        if (_serviceStateWorker.IsCategoryCurrent(categoryPath, page))
            return (true, _serviceStateWorker.Category);

        var (success, result) = await base.TryGetCategoryFromPathAsync(
            dataPath,
            requestData,
            categoryPath,
            page,
            token).ConfigureAwait(false);

        if (success && result != null)
            await _serviceStateWorker.SaveCategoryAsync(result, token).ConfigureAwait(false);

        return (success, result);
    }

    protected override async Task<bool> TryProcessCategoryProductAsync(
        ICategoryProductItem categoryProductItem,
        CancellationToken cancellationToken)
    {
        if (_serviceStateWorker.IsCategoryProductItemHandled(categoryProductItem))
            return true;

        var result = await base.TryProcessCategoryProductAsync(categoryProductItem, cancellationToken)
            .ConfigureAwait(false);

        if (result)
        {
            await _serviceStateWorker.SaveHandledCategoryProductItemAsync(categoryProductItem, cancellationToken)
                .ConfigureAwait(false);
        }

        return result;
    }

    protected override async Task InvokeConnectedAsync(
        bool connected,
        Exception? exception = null,
        CancellationToken cancellationToken = default)
    {
        await base.InvokeConnectedAsync(connected, exception, cancellationToken).ConfigureAwait(false);

        if (!connected && exception == null && _allCategoriesCompletedSuccessfully)
        {
            await _serviceStateWorker.RemoveAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    protected override async Task CloseAsync()
    {
        try
        {
            await base.CloseAsync().ConfigureAwait(false);
        }
        finally
        {
            await _serviceStateWorker.CloseAsync().ConfigureAwait(false);
        }
    }
}