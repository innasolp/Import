using Import.Catalog.Category.Factory.Interfaces;
using Import.Catalog.Category.Interfaces;
using Import.Factory.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCategoryService(this IServiceCollection services)
    {
        return services.AddScoped<IServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<CategoryServiceImpl>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            var loaderServiceFactories = sd.GetServices<ILoaderServiceFactory>();
            var treeItemLoaderFactories = sd.GetServices<ITreeItemLoaderFactory<CategoryTreeItem>>();
            var categoryItemHandlerFactories = sd.GetServices<ICategoryItemHandlerFactory<Interfaces.Category>>();
            return new CategoryServiceFactoryImpl(logger,
            loaderServiceFactories,
            treeItemLoaderFactories,
            categoryItemHandlerFactories,
            loggerFactory);
        });
    }

    public static IServiceCollection AddCategoryTimerService<TTreeItem, TCategory>(this IServiceCollection services)
    where TTreeItem : TreeItem.TreeItem, new()
    where TCategory : ICategory, new()
    {
        return services.AddScoped<IServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<CategoryTimerServiceImpl<TTreeItem, TCategory>>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            var loaderServiceFactories = sd.GetServices<ILoaderServiceFactory>();
            var treeItemLoaderFactories = sd.GetServices<ITreeItemLoaderFactory<TTreeItem>>();
            var categoryItemHandlerFactories = sd.GetServices<ICategoryItemHandlerFactory<TCategory>>();
            return new CategoryTimerServiceFactoryImpl<TTreeItem, TCategory>(logger,
            loaderServiceFactories,
            treeItemLoaderFactories,
            categoryItemHandlerFactories,
            loggerFactory);
        });
    }

    public static IServiceCollection AddCategoryTimerService(this IServiceCollection services)
    {
        return services.AddScoped<IServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<CategoryTimerServiceImpl>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            var loaderServiceFactories = sd.GetServices<ILoaderServiceFactory>();
            var treeItemLoaderFactories = sd.GetServices<ITreeItemLoaderFactory<CategoryTreeItem>>();
            var categoryItemHandlerFactories = sd.GetServices<ICategoryItemHandlerFactory<Interfaces.Category>>();
            return new CategoryTimerServiceFactoryImpl(logger,
            loaderServiceFactories,
            treeItemLoaderFactories,
            categoryItemHandlerFactories,
            loggerFactory);
        });
    }
}