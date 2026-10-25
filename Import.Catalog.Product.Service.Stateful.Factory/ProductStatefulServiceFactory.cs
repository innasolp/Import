using Import.Catalog.Product.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.KeyHash;
using Import.ServiceState;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Stateful.Factory;

public abstract class ProductStatefulServiceFactory<TCategory, TProductItem, TProductCatalogCategory>(
    ILogger logger,
    IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
    IEnumerable<IProductItemHandler> productItemHandlers,
    IEnumerable<ICategoryPaging<TCategory>> categoryPagingList,
    IEnumerable<IItemSerializer<TCategory>> categorySerializerList,
    IEnumerable<IKeyHasher> keyHasherList,
    IEnumerable<IServiceStateRepositoryFactory> serviceStateRepositoryFactoryList,
    IImportServiceLogFactory? logFactory = null)
    : IImportServiceFactory
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
    where TProductCatalogCategory : class, IProductCatalogCategory, new()
{
    public Type ServiceImplementationType => typeof(ProductStatefulServiceImpl<TCategory, TProductItem>);

    public IService Create(string name, IImportSource importSource, IImportSettings importSettings)
    {
        if (!importSettings.TryGetService<ProductServiceOptions>("ProductServiceOptions", out var productServiceOptions))
            throw new Exception($"Can't find {nameof(productServiceOptions)}.");

        if (productServiceOptions.ProductPathFormatType == default)
            throw new Exception($"Can't find {nameof(productServiceOptions.ProductPathFormatType)}.");

        if (productServiceOptions.CategoryPathFormatType == default)
            throw new Exception($"Can't find {nameof(productServiceOptions.CategoryPathFormatType)}.");

        var serviceLogger = logFactory?.GetLogger(logger, name, importSource, importSettings) ?? logger;

        var loaderFactory = loaderServiceFactories.GetFactoryBySettings(importSettings, "Loader", name);
        var loader = loaderFactory.Create(name, importSettings);

        var productItemHandler = productItemHandlers.GetItemBySettings(importSettings, "ProductItemHandler", name);

        var categoryPaging = categoryPagingList.GetItemBySettings(importSettings, "CategoryPaging", name);
        var categorySerializer = categorySerializerList.GetItemBySettings(importSettings, "CategorySerializer", name);
        var keyHasher = keyHasherList.GetItemBySettings(importSettings, "KeyHasher", name);
        var serviceStateRepositoryFactory = serviceStateRepositoryFactoryList.GetItemBySettings(importSettings, "StateRepositoryFactory", name);

        importSettings.TryGet<IEnumerable<TProductCatalogCategory>>("CatalogCategories", out var catalogCategories);

        if (!importSettings.TryGetString("ProductPathFormat", out var productPathFormat) || string.IsNullOrEmpty(productPathFormat))
            throw new Exception("Can't find ProductPathFormat.");
        if (!importSettings.TryGetString("CategoryPathFormat", out var categoryPathFormat) || string.IsNullOrEmpty(categoryPathFormat))
            throw new Exception("Can't find CategoryPathFormat.");

        importSettings.TryGetString("StateRepositoryConnectionString", out var stateConnectionString);
        var serviceStateRepository = string.IsNullOrEmpty(stateConnectionString)
            ? serviceStateRepositoryFactory.Create()
            : serviceStateRepositoryFactory.Create(stateConnectionString);

        return Create(
            serviceLogger,
            name,
            loader,
            catalogCategories ?? [],
            productItemHandler,
            productPathFormat ?? string.Empty,
            categoryPathFormat ?? string.Empty,
            importSource.Name,
            importSource.Url,
            categoryPaging,
            categorySerializer,
            productServiceOptions,
            keyHasher,
            serviceStateRepository);
    }

    public abstract Type GetServiceType();

    protected abstract ProductsStatefulService<TCategory, TProductItem> Create(
        ILogger logger,
        string name,
        ILoaderService loader,
        IEnumerable<IProductCatalogCategory> catalogCategories,
        IProductItemHandler productItemHandler,
        string productPathFormat,
        string categoryPathFormat,
        string sourceName,
        string url,
        ICategoryPaging<TCategory> categoryPaging,
        IItemSerializer<TCategory> categorySerializer,
        ProductServiceOptions productServiceOptions,
        IKeyHasher keyHasher,
        IServiceStateRepository serviceStateRepository);
}
