using Import.Catalog.Product.Interfaces;
using Import.ServiceState;

namespace Import.Catalog.Product.Service.Stateful;

internal class ServiceStateWorker<TCategory>(IServiceStateRepository serviceStateRepository)
    where TCategory : class, ICategoryProducts, new()
{
    private readonly IServiceStateRepository _serviceStateRepository = serviceStateRepository;
    private byte[] _serviceStateKey = [];
    private ServiceState<TCategory> _serviceState = new();
    private bool _isStarted;

    public CategoryProcessState CategoryProcessState => _serviceState.CategoryProcessState;
    public TCategory? Category => _serviceState.Category;
    public IProductCatalogCategory? ProductCatalogCategory => _serviceState.ProductCatalogCategory;

    public async Task Start(byte[] serviceStateKey, CancellationToken cancellationToken)
    {
        _serviceStateKey = serviceStateKey;

        if (!_serviceStateRepository.IsConnected)
            await _serviceStateRepository.Connect(cancellationToken).ConfigureAwait(false);

        _serviceState = await _serviceStateRepository
            .Load<ServiceState<TCategory>>(_serviceStateKey, cancellationToken)
            .ConfigureAwait(false) ?? new();

        _isStarted = true;
    }

    public Task ResetAsync(CancellationToken cancellationToken)
    {
        _serviceState.Reset();
        return _serviceStateRepository.Save(_serviceStateKey, _serviceState, cancellationToken);
    }

    public Task CompleteCategoryPageAsync(CancellationToken cancellationToken)
    {
        _serviceState.Category = null;
        _serviceState.CurrentCategoryProductItemId = null;
        _serviceState.HandledCategoryProductItemIds.Clear();
        return _serviceStateRepository.Save(_serviceStateKey, _serviceState, cancellationToken);
    }

    public Task SetProductCatalogCategoryIfNeedAsync(IProductCatalogCategory category, CancellationToken cancellationToken)
    {
        if (_serviceState.ProductCatalogCategory?.Path == category.Path &&
            _serviceState.ProductCatalogCategory?.ItemId == category.ItemId)
        {
            return Task.CompletedTask;
        }

        _serviceState.Reset();
        _serviceState.Start(category);
        return _serviceStateRepository.Save(_serviceStateKey, _serviceState, cancellationToken);
    }

    public bool IsCategoryCurrent(string categoryPath, int page)
    {
        return _serviceState.Category != null &&
               _serviceState.CategoryProcessState.CategoryPath == categoryPath &&
               _serviceState.CategoryProcessState.Page == page;
    }

    public Task SaveCategoryAsync(TCategory category, CancellationToken cancellationToken)
    {
        _serviceState.Category = category;
        return _serviceStateRepository.Save(_serviceStateKey, _serviceState, cancellationToken);
    }

    public bool IsCategoryProductItemHandled(ICategoryProductItem categoryProductItem)
    {
        return _serviceState.HandledCategoryProductItemIds.Contains(categoryProductItem.Id);
    }

    public Task SaveHandledCategoryProductItemAsync(
        ICategoryProductItem categoryProductItem,
        CancellationToken cancellationToken)
    {
        _serviceState.HandledCategoryProductItemIds.Add(categoryProductItem.Id);
        _serviceState.CurrentCategoryProductItemId = null;

        return _serviceStateRepository.Save(_serviceStateKey, _serviceState, cancellationToken);
    }

    public Task SetCurrentCategoryProductItemAsync(
        ICategoryProductItem categoryProductItem,
        CancellationToken cancellationToken)
    {
        _serviceState.CurrentCategoryProductItemId = categoryProductItem.Id;
        return _serviceStateRepository.Save(_serviceStateKey, _serviceState, cancellationToken);
    }

    public async Task RemoveAsync(CancellationToken cancellationToken = default)
    {
        if (!_isStarted || _serviceStateKey.Length == 0)
            return;

        await _serviceStateRepository.Remove(_serviceStateKey, cancellationToken).ConfigureAwait(false);
    }

    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (_serviceStateRepository.IsConnected)
            await _serviceStateRepository.Close(cancellationToken).ConfigureAwait(false);
    }
}