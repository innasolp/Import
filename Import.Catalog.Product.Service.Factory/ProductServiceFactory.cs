using Import.Catalog.Product.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Factory;

public abstract class ProductServiceFactory<TLoadOptions, TProductService, TCategory, TProductItem, TProductCatalogCategory, TProductServiceOptions>(
    ILogger<TProductService> logger,
    IEnumerable<ILoaderServiceFactory<LoadData<TLoadOptions>>> loaderServiceFactories,
    IEnumerable<IFactory<IProductItemHandler>> productItemHandlerFactories,
    IEnumerable<ICategoryPaging<TCategory>> categoryPagingList,
    IEnumerable<IItemSerializer<TCategory>> categorySerializerList,
    IEnumerable<IFactory<IItemSerializer<TCategory>>> categorySerializerFactoryList,
    IEnumerable<IFactory<IItemPathFormatter<ICategoryProductItem>>>? itemPathFormatterFactoryList = null,
    IImportServiceLogFactory? logFactory = null)
    : IServiceFactory
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
    where TProductCatalogCategory : class, IProductCatalogCategory, new()
    where TProductServiceOptions : ProductServiceOptions<TLoadOptions>, new()
    where TProductService : ProductService<TLoadOptions, TCategory, TProductItem>
    where TLoadOptions : class
{
    public const string ProductItemHandler = "ProductItemHandler";
    public const string ProductServiceOptions = "ProductServiceOptions";
    public const string Loader = "Loader";
    public const string CategoryPaging = "CategoryPaging";
    public const string CategorySerializer = "CategorySerializer";
    public const string CatalogCategories = "CatalogCategories";
    public const string ProductPathFormat = "ProductPathFormat";
    public const string CategoryPathFormat = "CategoryPathFormat";
    public const string ItemPathFormatter = "ItemPathFormatter";

    public abstract Type GetServiceType();

    protected abstract TProductService Create(IImportSettingsProvider importSettingsProvider, IImportSettings importSettings,
        ILogger<TProductService> logger,
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
        TProductServiceOptions productServiceOptions);

    public IService Create(string name, IImportSource importSource, IImportSettingsProvider importSettingsProvider, IImportSettings importSettings)
    {
        var serviceLogger = logFactory?.GetLogger(logger, name, importSource, importSettingsProvider, importSettings) ?? logger;

        if (!importSettingsProvider.TryGetService<TProductServiceOptions>(importSettings, ProductServiceOptions, out var productServiceOptions)
        || productServiceOptions == null)
            throw new Exception($"Can't find {nameof(productServiceOptions)}.");

        if (productServiceOptions?.ProductPathFormatType == default)
            throw new InvalidDataException($"Can't find {nameof(productServiceOptions.ProductPathFormatType)}.");

        if (productServiceOptions?.CategoryPathFormatType == default)
            throw new InvalidDataException($"Can't find {nameof(productServiceOptions.CategoryPathFormatType)}.");

        if(!loaderServiceFactories.TryGetFactoryBySettings(importSettingsProvider, importSettings, Loader, out var loaderFactory)
            || loaderFactory == null)
             throw new InvalidDataException($"Can't find {nameof(Loader)} in {name}.");
        var loader = loaderFactory.Create(name, importSettingsProvider, importSettings);

        if(!productItemHandlerFactories.TryGetFactoryBySettings(importSettingsProvider, importSettings, ProductItemHandler, out var productItemHandlerFactory)
           || productItemHandlerFactory==null)
            throw new InvalidDataException($"Can't find {nameof(ProductItemHandler)}.");
        var productItemHandler = productItemHandlerFactory.Create(importSettingsProvider, importSettings);

        if(!categoryPagingList.TryGetItemBySettings(importSettingsProvider, importSettings, CategoryPaging, out var categoryPaging)
            || categoryPaging == null)
          throw new InvalidDataException($"Can't find {nameof(CategoryPaging)}.");

        if(!categorySerializerList.TryGetItemBySettings(importSettingsProvider, importSettings, CategorySerializer, out var categorySerializer)
           || categorySerializer == null)
        { 
           if(!categorySerializerFactoryList.TryGetFactoryBySettings(importSettingsProvider, importSettings, CategorySerializer, out var categorySerializerFactory)
             || categorySerializerFactory == null)
             throw new InvalidDataException($"Can't find {nameof(CategorySerializer)} in {name}.");
           
           categorySerializer = categorySerializerFactory.Create(importSettingsProvider, importSettings);
        } 

        if (!importSettingsProvider.TryGet<TProductCatalogCategory[]>(importSettings, CatalogCategories, out var catalogCategories)
        || catalogCategories == null)
            throw new InvalidDataException($"Can't find {nameof(CatalogCategories)}.");

        if (!importSettingsProvider.TryGetString(importSettings, ProductPathFormat, out var productPathFormat) || string.IsNullOrEmpty(productPathFormat))
            throw new InvalidDataException($"Can't find {nameof(ProductPathFormat)}.");

        if (!importSettingsProvider.TryGetString(importSettings, CategoryPathFormat, out var categoryPathFormat) || string.IsNullOrEmpty(categoryPathFormat))
            throw new InvalidDataException($"Can't find {nameof(CategoryPathFormat)}.");

        if(!itemPathFormatterFactoryList.TryGetFactoryBySettings(importSettingsProvider, importSettings, ItemPathFormatter, out var itemPathFormatterFactory)
           || itemPathFormatterFactory == null) 
            throw new InvalidDataException($"Can't find {nameof(ItemPathFormatter)} in {name}.");
        var itemPathFormatter = itemPathFormatterFactory.Create(importSettingsProvider, importSettings);

        return Create(importSettingsProvider, importSettings,
            serviceLogger,
            name,
            loader,
            catalogCategories ?? [],
            productItemHandler,
            productPathFormat,
            categoryPathFormat,
            importSource.Name,
            importSource.Url,
            categoryPaging,
            categorySerializer,
            itemPathFormatter,
            productServiceOptions);
    }
}