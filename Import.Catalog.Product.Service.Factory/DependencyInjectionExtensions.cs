using Import.Catalog.Product.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Factory;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddProductService<TLoadOptions, TCategory, TProductItem, TProductCatalogCategory, TProductServiceOptions>
    (this IServiceCollection services)
        where TCategory : class, ICategoryProducts, new()
        where TProductItem : class, IProductItem, new()
        where TProductCatalogCategory : class, IProductCatalogCategory, new()
        where TProductServiceOptions : ProductServiceOptions<TLoadOptions>, new()
        where TLoadOptions : class
    {
        return services.AddScoped<IServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<ProductServiceImpl<TLoadOptions, TCategory, TProductItem>>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            var loaderServiceFactories = sd.GetServices<ILoaderServiceFactory<LoadData<TLoadOptions>>>();
            var productItemHandlerFactories = sd.GetServices<IFactory<IProductItemHandler>>();
            var categoryPagingList = sd.GetServices<ICategoryPaging<TCategory>>();
            var categorySerializerList = sd.GetServices<IItemSerializer<TCategory>>();
            var categorySerializerFactoryList = sd.GetServices<IFactory<IItemSerializer<TCategory>>>();

            return new ProductServiceFactoryImpl<TLoadOptions, TCategory, TProductItem, TProductCatalogCategory, TProductServiceOptions>(
                logger,
                loaderServiceFactories,
                productItemHandlerFactories,
                categoryPagingList,
                categorySerializerList,
                categorySerializerFactoryList,
                loggerFactory);
        });
    }

    public static IServiceCollection AddProductService<TLoadOptions, TCategory, TProductItem, TProductServiceOptions>(this IServiceCollection services)
        where TCategory : class, ICategoryProducts, new()
        where TProductItem : class, IProductItem, new()
        where TProductServiceOptions : ProductServiceOptions<TLoadOptions>, new()
        where TLoadOptions : class
    {
        return services.AddScoped<IServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<ProductServiceImpl<TLoadOptions, TCategory, TProductItem>>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            var loaderServiceFactories = sd.GetServices<ILoaderServiceFactory<LoadData<TLoadOptions>>>();
            var productItemHandlerFactories = sd.GetServices<IFactory<IProductItemHandler>>();
            var categoryPagingList = sd.GetServices<ICategoryPaging<TCategory>>();
            var categorySerializerList = sd.GetServices<IItemSerializer<TCategory>>();
            var categorySerializerFactoryList = sd.GetServices<IFactory<IItemSerializer<TCategory>>>();

            return new ProductServiceFactoryImpl<TLoadOptions, TCategory, TProductItem, ProductCatalogCategoryImpl, TProductServiceOptions>(
                logger,
                loaderServiceFactories,
                productItemHandlerFactories,
                categoryPagingList,
                categorySerializerList,
                categorySerializerFactoryList,
                loggerFactory);
        });
    }
}