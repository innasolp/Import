using Import.Catalog.Product.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.KeyHash;
using Import.ServiceState;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Stateful.Factory;

public class ProductStatefulServiceFactoryImpl<TCategory, TProductItem, TProductCatalogCategory>(
    ILogger logger,
    IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
    IEnumerable<IProductItemHandler> productItemHandlers,
    IEnumerable<ICategoryPaging<TCategory>> categoryPagingList,
    IEnumerable<IItemSerializer<TCategory>> categorySerializerList,
    IEnumerable<IKeyHasher> keyHasherList,
    IEnumerable<IServiceStateRepositoryFactory> serviceStateRepositoryFactoryList,
    IImportServiceLogFactory? logFactory = null)
    : ProductStatefulServiceFactory<TCategory, TProductItem, TProductCatalogCategory>(
        logger,
        loaderServiceFactories,
        productItemHandlers,
        categoryPagingList,
        categorySerializerList,
        keyHasherList,
        serviceStateRepositoryFactoryList,
        logFactory)
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
    where TProductCatalogCategory : class, IProductCatalogCategory, new()
{
    public override Type GetServiceType()
    {
        return typeof(ProductStatefulServiceImpl<TCategory, TProductItem>);
    }

    protected override ProductsStatefulService<TCategory, TProductItem> Create(
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
        IServiceStateRepository serviceStateRepository)
    {
        return new ProductStatefulServiceImpl<TCategory, TProductItem>(
            logger,
            name,
            loader,
            url,
            catalogCategories,
            productItemHandler,
            productPathFormat,
            categoryPathFormat,
            sourceName,
            categoryPaging,
            categorySerializer,
            productServiceOptions,
            keyHasher,
            serviceStateRepository);
    }
}
