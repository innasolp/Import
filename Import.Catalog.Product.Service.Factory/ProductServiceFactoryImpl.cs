using Import.Catalog.Product.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Factory;

public class ProductServiceFactoryImpl<TCategory, TProductItem, TProductCatalogCategory>(
    ILogger logger,
    IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
    IEnumerable<IProductItemHandler> productItemHandlers,
    IEnumerable<ICategoryPaging<TCategory>> categoryPagingList,
    IEnumerable<IItemSerializer<TCategory>> categorySerializerList,
    IImportServiceLogFactory? logFactory = null)
    : ProductServiceFactory<TCategory, TProductItem, TProductCatalogCategory>(
        logger,
        loaderServiceFactories,
        productItemHandlers,
        categoryPagingList,
        categorySerializerList,
        logFactory)
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
    where TProductCatalogCategory : class, IProductCatalogCategory, new()
{
    public override Type GetServiceType()
    {
        return typeof(ProductServiceImpl<TCategory, TProductItem>);
    }

    protected override ProductService<TCategory, TProductItem> Create(
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
        ProductServiceOptions productServiceOptions)
    {
        return new ProductServiceImpl<TCategory, TProductItem>(
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
            productServiceOptions);
    }
}
