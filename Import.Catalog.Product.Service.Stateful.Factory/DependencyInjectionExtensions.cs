using Import.Catalog.Product.Interfaces;
using Import.Catalog.Product.Service;
using Import.Catalog.Product.Service.Stateful;
using Import.Factory.Interfaces;
using Import.KeyHash;
using Import.ServiceState;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Stateful.Factory;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddProductStatefulService<TCategory, TProductItem, TProductCatalogCategory>(this IServiceCollection services)
        where TCategory : class, ICategoryProducts, new()
        where TProductItem : class, IProductItem, new()
        where TProductCatalogCategory : class, IProductCatalogCategory, new()
    {
        return services.AddScoped<IImportServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<ProductStatefulServiceImpl<TCategory, TProductItem>>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            var loaderServiceFactories = sd.GetServices<ILoaderServiceFactory>();
            var productItemHandlers = sd.GetServices<IProductItemHandler>();
            var categoryPagingList = sd.GetServices<ICategoryPaging<TCategory>>();
            var categorySerializerList = sd.GetServices<IItemSerializer<TCategory>>();
            var keyHasherList = sd.GetServices<IKeyHasher>();
            var serviceStateRepositoryFactoryList = sd.GetServices<IServiceStateRepositoryFactory>();

            return new ProductStatefulServiceFactoryImpl<TCategory, TProductItem, TProductCatalogCategory>(
                logger,
                loaderServiceFactories,
                productItemHandlers,
                categoryPagingList,
                categorySerializerList,
                keyHasherList,
                serviceStateRepositoryFactoryList,
                loggerFactory);
        });
    }

    public static IServiceCollection AddProductStatefulService<TCategory, TProductItem>(this IServiceCollection services)
        where TCategory : class, ICategoryProducts, new()
        where TProductItem : class, IProductItem, new()
    {
        return services.AddScoped<IImportServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<ProductStatefulServiceImpl<TCategory, TProductItem>>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            var loaderServiceFactories = sd.GetServices<ILoaderServiceFactory>();
            var productItemHandlers = sd.GetServices<IProductItemHandler>();
            var categoryPagingList = sd.GetServices<ICategoryPaging<TCategory>>();
            var categorySerializerList = sd.GetServices<IItemSerializer<TCategory>>();
            var keyHasherList = sd.GetServices<IKeyHasher>();
            var serviceStateRepositoryFactoryList = sd.GetServices<IServiceStateRepositoryFactory>();

            return new ProductStatefulServiceFactoryImpl<TCategory, TProductItem, ProductCatalogCategoryImpl>(
                logger,
                loaderServiceFactories,
                productItemHandlers,
                categoryPagingList,
                categorySerializerList,
                keyHasherList,
                serviceStateRepositoryFactoryList,
                loggerFactory);
        });
    }
}
