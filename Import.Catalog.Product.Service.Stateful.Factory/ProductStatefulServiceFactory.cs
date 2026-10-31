using Import.Catalog.Product.Factory.Interfaces;
using Import.Catalog.Product.Interfaces;
using Import.Catalog.Product.Service.Factory;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.KeyHash;
using Import.ServiceState;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Stateful.Factory;

public abstract class ProductStatefulServiceFactory<TProductStatefulService,
 TCategory,
 TProductItem,
 TProductCatalogCategory,
 TProductServiceOptions>(
    ILogger<TProductStatefulService> logger,
    IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
    IEnumerable<IProductItemHandlerFactory> productItemHandlerFactories,
    IEnumerable<ICategoryPaging<TCategory>> categoryPagingList,
    IEnumerable<IItemSerializer<TCategory>> categorySerializerList,
    IEnumerable<IKeyHasher> keyHasherList,
    IEnumerable<IServiceStateRepositoryFactory> serviceStateRepositoryFactoryList,
    IImportServiceLogFactory? logFactory = null)
    : ProductServiceFactory<TProductStatefulService, TCategory, TProductItem, TProductCatalogCategory, TProductServiceOptions>
    (logger, loaderServiceFactories, productItemHandlerFactories, categoryPagingList, categorySerializerList, logFactory)
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
    where TProductCatalogCategory : class, IProductCatalogCategory, new()
    where TProductServiceOptions : ProductServiceOptions
    where TProductStatefulService : ProductStatefulService<TCategory, TProductItem>
{
    public const string KeyHasher = "KeyHasher";

    public const string ServiceState = "ServiceState";

    protected override TProductStatefulService Create(IImportSettingsProvider importSettingsProvider, IImportSettings importSettings,
    ILogger<TProductStatefulService> logger,
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
      TProductServiceOptions productServiceOptions)
    {
        var keyHasher = keyHasherList.GetItemBySettings(importSettingsProvider, importSettings, KeyHasher, name)
        ?? throw new Exception($"Can't find {nameof(KeyHasher)}.");

        if (!importSettingsProvider.TryGetService<ServiceStateSettings>(importSettings, ServiceState, out var serviceStateOptions)
            || serviceStateOptions == null)
            throw new Exception($"Can't find {nameof(ServiceState)}.");

        var serviceStateRepositoryFactory = serviceStateRepositoryFactoryList.GetItemBySettings(importSettingsProvider, importSettings, serviceStateOptions.Provider, name);

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
            productServiceOptions,
            keyHasher,
            serviceStateRepository);
    }

    protected abstract TProductStatefulService Create(IImportSettingsProvider importSettingsProvider, IImportSettings importSettings,
    ILogger<TProductStatefulService> logger,
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
      TProductServiceOptions productServiceOptions,
      IKeyHasher keyHasher,
      IServiceStateRepository serviceStateRepository);
}