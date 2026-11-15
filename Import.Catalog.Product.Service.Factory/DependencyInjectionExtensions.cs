using Import.Catalog.Product.Factory.Interfaces;
using Import.Catalog.Product.Interfaces;
using Import.Factory.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Factory;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddProductService<TCategory, TProductItem, TProductCatalogCategory, TProductServiceOptions>
    (this IServiceCollection services)
        where TCategory : class, ICategoryProducts, new()
        where TProductItem : class, IProductItem, new()
        where TProductCatalogCategory : class, IProductCatalogCategory, new()
        where TProductServiceOptions : ProductServiceOptions
    {
        return services.AddScoped<IServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<ProductServiceImpl<TCategory, TProductItem>>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            var loaderServiceFactories = sd.GetServices<ILoaderServiceFactory>();
            var productItemHandlerFactories = sd.GetServices<IProductItemHandlerFactory>();
            var categoryPagingList = sd.GetServices<ICategoryPaging<TCategory>>();
            var categorySerializerList = sd.GetServices<IItemSerializer<TCategory>>();

            return new ProductServiceFactoryImpl<TCategory, TProductItem, TProductCatalogCategory, TProductServiceOptions>(
                logger,
                loaderServiceFactories,
                productItemHandlerFactories,
                categoryPagingList,
                categorySerializerList,
                loggerFactory);
        });
    }

    public static IServiceCollection AddProductService<TCategory, TProductItem, TProductServiceOptions>(this IServiceCollection services)
        where TCategory : class, ICategoryProducts, new()
        where TProductItem : class, IProductItem, new()
        where TProductServiceOptions : ProductServiceOptions
    {
        return services.AddScoped<IServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<ProductServiceImpl<TCategory, TProductItem>>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            var loaderServiceFactories = sd.GetServices<ILoaderServiceFactory>();
            var productItemHandlerFactories = sd.GetServices<IProductItemHandlerFactory>();
            var categoryPagingList = sd.GetServices<ICategoryPaging<TCategory>>();
            var categorySerializerList = sd.GetServices<IItemSerializer<TCategory>>();

            return new ProductServiceFactoryImpl<TCategory, TProductItem, ProductCatalogCategoryImpl, TProductServiceOptions>(
                logger,
                loaderServiceFactories,
                productItemHandlerFactories,
                categoryPagingList,
                categorySerializerList,
                loggerFactory);
        });
    }
}