using Import.Catalog.Product.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Factory;

internal class ProductServiceFactoryImpl<TLoadOptions, TCategory, TProductItem, TProductCatalogCategory, TProductServiceOptions>(
    ILogger<ProductServiceImpl<TLoadOptions, TCategory, TProductItem>> logger,
    IEnumerable<ILoaderServiceFactory<LoadData<TLoadOptions>>> loaderServiceFactories,
    IEnumerable<IFactory<IProductItemHandler>> productItemHandlerFactories,
    IEnumerable<ICategoryPaging<TCategory>> categoryPagingList,
    IEnumerable<IItemSerializer<TCategory>> categorySerializerList,
    IEnumerable<IFactory<IItemSerializer<TCategory>>> categorySerializerFactoryList,
    IImportServiceLogFactory? logFactory = null)
    : ProductServiceFactory<TLoadOptions, ProductServiceImpl<TLoadOptions, TCategory, TProductItem>, TCategory, TProductItem, TProductCatalogCategory, TProductServiceOptions>(
        logger,
        loaderServiceFactories,
        productItemHandlerFactories,
        categoryPagingList,
        categorySerializerList,
        categorySerializerFactoryList,
        logFactory)
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
    where TProductCatalogCategory : class, IProductCatalogCategory, new()
    where TProductServiceOptions : ProductServiceOptions<TLoadOptions>, new()
    where TLoadOptions : class
{
    public override Type GetServiceType()
    {
        return typeof(ProductServiceImpl<TLoadOptions, TCategory, TProductItem>);
    }

    protected override ProductServiceImpl<TLoadOptions, TCategory, TProductItem> Create(IImportSettingsProvider importSettingsProvider, IImportSettings importSettings,
        ILogger<ProductServiceImpl<TLoadOptions, TCategory, TProductItem>> logger,
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
        TProductServiceOptions productServiceOptions)
    {
        return new ProductServiceImpl<TLoadOptions, TCategory, TProductItem>(
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
