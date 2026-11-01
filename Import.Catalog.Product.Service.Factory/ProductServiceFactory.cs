using Import.Catalog.Product.Factory.Interfaces;
using Import.Catalog.Product.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Factory;

public abstract class ProductServiceFactory<TProductService, TCategory, TProductItem, TProductCatalogCategory, TProductServiceOptions>(
    ILogger<TProductService> logger,
    IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
    IEnumerable<IProductItemHandlerFactory> productItemHandlerFactories,
    IEnumerable<ICategoryPaging<TCategory>> categoryPagingList,
    IEnumerable<IItemSerializer<TCategory>> categorySerializerList,
    IImportServiceLogFactory? logFactory = null)
    : IServiceFactory
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
    where TProductCatalogCategory : class, IProductCatalogCategory, new()
    where TProductServiceOptions : ProductServiceOptions
    where TProductService : ProductService<TCategory, TProductItem>
{
    public const string ProductItemHandler = "ProductItemHandler";
    public const string ProductServiceOptions = "ProductServiceOptions";
    public const string Loader = "Loader";
    public const string CategoryPaging = "CategoryPaging";
    public const string CategorySerializer = "CategorySerializer";
    public const string CatalogCategories = "CatalogCategories";
    public const string ProductPathFormat = "ProductPathFormat";
    public const string CategoryPathFormat = "CategoryPathFormat";

    public abstract Type GetServiceType();

    protected abstract TProductService Create(IImportSettingsProvider importSettingsProvider, IImportSettings importSettings,
        ILogger<TProductService> logger,
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
        TProductServiceOptions productServiceOptions);

    public IService Create(string name, IImportSource importSource, IImportSettingsProvider importSettingsProvider, IImportSettings importSettings)
    {
        var serviceLogger = logFactory?.GetLogger(logger, name, importSource, importSettings) ?? logger;

        if (!importSettingsProvider.TryGetService<TProductServiceOptions>(importSettings, ProductServiceOptions, out var productServiceOptions)
        || productServiceOptions == null)
            throw new Exception($"Can't find {nameof(productServiceOptions)}.");

        if (productServiceOptions?.ProductPathFormatType == default)
            throw new Exception($"Can't find {nameof(productServiceOptions.ProductPathFormatType)}.");

        if (productServiceOptions?.CategoryPathFormatType == default)
            throw new Exception($"Can't find {nameof(productServiceOptions.CategoryPathFormatType)}.");

        var loaderFactory = loaderServiceFactories.GetFactoryBySettings(importSettingsProvider, importSettings, Loader, name)
            ?? throw new Exception($"Can't find {nameof(Loader)}.");
        var loader = loaderFactory.Create(name, importSettings);

        var productItemHandlerFactory = productItemHandlerFactories.GetFactoryBySettings(importSettingsProvider, importSettings, ProductItemHandler, name)
            ?? throw new Exception($"Can't find {nameof(ProductItemHandler)}.");
        var productItemHandler = productItemHandlerFactory.Create(importSettingsProvider, importSettings);

        var categoryPaging = categoryPagingList.GetItemBySettings(importSettingsProvider, importSettings, CategoryPaging, name)
        ?? throw new Exception($"Can't find {nameof(CategoryPaging)}.");

        var categorySerializer = categorySerializerList.GetItemBySettings(importSettingsProvider, importSettings, CategorySerializer, name)
            ?? throw new Exception($"Can't find {nameof(CategorySerializer)}.");

        if (!importSettingsProvider.TryGet<IEnumerable<TProductCatalogCategory>>(importSettings, CatalogCategories, out var catalogCategories)
        || catalogCategories == null)
            throw new Exception($"Can't find {nameof(CatalogCategories)}.");

        if (!importSettingsProvider.TryGetString(importSettings, ProductPathFormat, out var productPathFormat) || string.IsNullOrEmpty(productPathFormat))
            throw new Exception($"Can't find {nameof(ProductPathFormat)}.");

        if (!importSettingsProvider.TryGetString(importSettings, CategoryPathFormat, out var categoryPathFormat) || string.IsNullOrEmpty(categoryPathFormat))
            throw new Exception($"Can't find {nameof(CategoryPathFormat)}.");

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
            productServiceOptions);
    }
}