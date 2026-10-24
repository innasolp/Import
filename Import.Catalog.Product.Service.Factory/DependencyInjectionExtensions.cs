using Import.Catalog.Product.Interfaces;
using Import.Catalog.Product.Service;
using Import.Factory.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Factory;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddProductService<TCategory, TProductItem, TProductCatalogCategory>(this IServiceCollection services)
        where TCategory : class, ICategoryProducts, new()
        where TProductItem : class, IProductItem, new()
        where TProductCatalogCategory : class, IProductCatalogCategory, new()
    {
        return services.AddScoped<IImportServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<ProductServiceImpl<TCategory, TProductItem>>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            var loaderServiceFactories = sd.GetServices<ILoaderServiceFactory>();
            var productItemHandlers = sd.GetServices<IProductItemHandler>();
            var categoryPagingList = sd.GetServices<ICategoryPaging<TCategory>>();
            var categorySerializerList = sd.GetServices<IItemSerializer<TCategory>>();

            return new ProductServiceFactoryImpl<TCategory, TProductItem, TProductCatalogCategory>(
                logger,
                loaderServiceFactories,
                productItemHandlers,
                categoryPagingList,
                categorySerializerList,
                loggerFactory);
        });
    }

    public static IServiceCollection AddProductService<TCategory, TProductItem>(this IServiceCollection services)
        where TCategory : class, ICategoryProducts, new()
        where TProductItem : class, IProductItem, new()
    {
        return services.AddScoped<IImportServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<ProductServiceImpl<TCategory, TProductItem>>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            var loaderServiceFactories = sd.GetServices<ILoaderServiceFactory>();
            var productItemHandlers = sd.GetServices<IProductItemHandler>();
            var categoryPagingList = sd.GetServices<ICategoryPaging<TCategory>>();
            var categorySerializerList = sd.GetServices<IItemSerializer<TCategory>>();

            return new ProductServiceFactoryImpl<TCategory, TProductItem, ProductCatalogCategoryImpl>(
                logger,
                loaderServiceFactories,
                productItemHandlers,
                categoryPagingList,
                categorySerializerList,
                loggerFactory);
        });
    }
}
