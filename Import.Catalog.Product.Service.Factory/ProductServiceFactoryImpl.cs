using Import.Catalog.Product.Factory.Interfaces;
using Import.Catalog.Product.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Factory;

internal class ProductServiceFactoryImpl<TCategory, TProductItem, TProductCatalogCategory, TProductServiceOptions>(
    ILogger<ProductServiceImpl<TCategory, TProductItem>> logger,
    IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
    IEnumerable<IProductItemHandlerFactory> productItemHandlerFactories,
    IEnumerable<ICategoryPaging<TCategory>> categoryPagingList,
    IEnumerable<IItemSerializer<TCategory>> categorySerializerList,
    IImportServiceLogFactory? logFactory = null)
    : ProductServiceFactory<ProductServiceImpl<TCategory, TProductItem>, TCategory, TProductItem, TProductCatalogCategory, TProductServiceOptions>(
        logger,
        loaderServiceFactories,
        productItemHandlerFactories,
        categoryPagingList,
        categorySerializerList,
        logFactory)
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
    where TProductCatalogCategory : class, IProductCatalogCategory, new()
    where TProductServiceOptions : ProductServiceOptions
{
    public override Type GetServiceType()
    {
        return typeof(ProductServiceImpl<TCategory, TProductItem>);
    }

    protected override ProductServiceImpl<TCategory, TProductItem> Create(IImportSettingsProvider importSettingsProvider, IImportSettings importSettings,
        ILogger<ProductServiceImpl<TCategory, TProductItem>> logger,
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
        return new ProductServiceImpl<TCategory, TProductItem>(
           logger,
           name,
           loader,
           url,
           catalogCategories,
           productItemHandler,
           productPathFormat,
           categoryPathFormat,
           name,
           categoryPaging,
           categorySerializer,
           productServiceOptions);
    }
}
