using Import.Catalog.Product.Interfaces;
using Import.Catalog.Product.Service.Factory;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.KeyHash;
using Import.ServiceState;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Stateful.Factory;

public abstract class ProductStatefulServiceFactory<TLoadOptions, TProductStatefulService,
 TCategory,
 TProductItem,
 TProductCatalogCategory,
 TProductServiceOptions>(
    ILogger<TProductStatefulService> logger,
    IEnumerable<ILoaderServiceFactory<LoadData<TLoadOptions>>> loaderServiceFactories,
    IEnumerable<IFactory<IProductItemHandler>> productItemHandlerFactories,
    IEnumerable<ICategoryPaging<TCategory>> categoryPagingList,
    IEnumerable<IItemSerializer<TCategory>> categorySerializerList,
    IEnumerable<IFactory<IItemSerializer<TCategory>>> categorySerializerFactoryList,
    IEnumerable<IFactory<IItemPathFormatter<ICategoryProductItem>>> itemPathFormatterFactoryList,
    IEnumerable<IKeyHasher> keyHasherList,
    IEnumerable<IServiceStateRepositoryFactory> serviceStateRepositoryFactoryList,
    IImportServiceLogFactory? logFactory = null)
    : ProductServiceFactory<TLoadOptions, TProductStatefulService, TCategory, TProductItem, TProductCatalogCategory, TProductServiceOptions>
    (logger, loaderServiceFactories, productItemHandlerFactories, categoryPagingList, categorySerializerList, categorySerializerFactoryList, itemPathFormatterFactoryList, logFactory)
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
    where TProductCatalogCategory : class, IProductCatalogCategory, new()
    where TProductServiceOptions : ProductServiceOptions<TLoadOptions>, new()
    where TProductStatefulService : ProductStatefulService<TLoadOptions, TCategory, TProductItem>
    where TLoadOptions : class
{
    public const string KeyHasher = "KeyHasher";

    public const string ServiceState = "ServiceState";

    protected override TProductStatefulService Create(IImportSettingsProvider importSettingsProvider, IImportSettings importSettings,
    ILogger<TProductStatefulService> logger,
    string name,
    ILoaderService<LoadData<TLoadOptions>> loader,
    IEnumerable<IProductCatalogCategory> catalogCategories,
    IProductItemHandler productItemHandler,
    string productPathFormat,
    string categoryPathFormat,
    string sourceName,
    string url,
     ICategoryPaging<TCategory> categoryPaging,
     IItemSerializer<TCategory> categorySerializer,
     IItemPathFormatter<ICategoryProductItem> productItemPathFormatter,
      TProductServiceOptions productServiceOptions)
    {
        var keyHasher = keyHasherList.GetItemBySettings(importSettingsProvider, importSettings, KeyHasher, name)
        ?? throw new Exception($"Can't find {nameof(KeyHasher)}.");

        if (!importSettingsProvider.TryGetService<ServiceStateSettings>(importSettings, ServiceState, out var serviceStateOptions)
            || serviceStateOptions == null)
            throw new Exception($"Can't find {nameof(ServiceState)}.");

        if (string.IsNullOrEmpty(serviceStateOptions.Provider))
            throw new InvalidDataException($"Can't find {nameof(ServiceStateSettings.Provider)} in {nameof(ServiceState)}.");

        var serviceStateRepositoryFactory = serviceStateRepositoryFactoryList.GetFactoryByImplementationName(
            serviceStateOptions.Provider,
            name);

        var serviceStateRepository = string.IsNullOrEmpty(serviceStateOptions.ConnectionString)
            ? serviceStateRepositoryFactory.Create()
            : serviceStateRepositoryFactory.Create(serviceStateOptions.ConnectionString);

        return Create(importSettingsProvider, importSettings,
            logger,
            name,
            loader,
            catalogCategories ?? [],
            productItemHandler,
            productPathFormat,
            categoryPathFormat,
            sourceName,
            url,
            categoryPaging,
            categorySerializer,
            productItemPathFormatter,
            productServiceOptions,
            keyHasher,
            serviceStateRepository);
    }

    protected abstract TProductStatefulService Create(IImportSettingsProvider importSettingsProvider, IImportSettings importSettings,
    ILogger<TProductStatefulService> logger,
    string name,
    ILoaderService<LoadData<TLoadOptions>> loader,
    IEnumerable<IProductCatalogCategory> catalogCategories,
    IProductItemHandler productItemHandler,
    string productPathFormat,
    string categoryPathFormat,
    string sourceName,
    string url,
     ICategoryPaging<TCategory> categoryPaging,
     IItemSerializer<TCategory> categorySerializer,
     IItemPathFormatter<ICategoryProductItem> productItemPathFormatter,
      TProductServiceOptions productServiceOptions,
      IKeyHasher keyHasher,
      IServiceStateRepository serviceStateRepository);
}
