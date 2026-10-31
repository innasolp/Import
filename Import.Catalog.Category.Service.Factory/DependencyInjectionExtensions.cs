using Import.Catalog.Category.Factory.Interfaces;
using Import.Factory.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCategoryService<TTreeItem>(this IServiceCollection services)
    where TTreeItem : TreeItem.TreeItem, new()
    {
        return services.AddScoped<IServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<CategoryServiceImpl<TTreeItem>>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            var loaderServiceFactories = sd.GetServices<ILoaderServiceFactory>();
            var treeItemLoaderFactories = sd.GetServices<ITreeItemLoaderFactory<TTreeItem>>();
            var categoryItemHandlerFactories = sd.GetServices<ICategoryItemHandlerFactory<TTreeItem>>();
            return new CategoryServiceFactoryImpl<TTreeItem>(logger,
            loaderServiceFactories,
            treeItemLoaderFactories,
            categoryItemHandlerFactories,
            loggerFactory);
        });
    }

    public static IServiceCollection AddCategoryService(this IServiceCollection services)
    {
        return services.AddScoped<IServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<CategoryServiceImpl>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            var loaderServiceFactories = sd.GetServices<ILoaderServiceFactory>();
            var treeItemLoaderFactories = sd.GetServices<ITreeItemLoaderFactory<Category>>();
            var categoryItemHandlerFactories = sd.GetServices<ICategoryItemHandlerFactory<Category>>();
            return new CategoryServiceFactoryImpl(logger,
            loaderServiceFactories,
            treeItemLoaderFactories,
            categoryItemHandlerFactories,
            loggerFactory);
        });
    }

    public static IServiceCollection AddCategoryTimerService<TTreeItem>(this IServiceCollection services)
    where TTreeItem : TreeItem.TreeItem, new()
    {
        return services.AddScoped<IServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<CategoryTimerServiceImpl<TTreeItem>>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            var loaderServiceFactories = sd.GetServices<ILoaderServiceFactory>();
            var treeItemLoaderFactories = sd.GetServices<ITreeItemLoaderFactory<TTreeItem>>();
            var categoryItemHandlerFactories = sd.GetServices<ICategoryItemHandlerFactory<TTreeItem>>();
            return new CategoryTimerServiceFactoryImpl<TTreeItem>(logger,
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
            var treeItemLoaderFactories = sd.GetServices<ITreeItemLoaderFactory<Category>>();
            var categoryItemHandlerFactories = sd.GetServices<ICategoryItemHandlerFactory<Category>>();
            return new CategoryTimerServiceFactoryImpl<Category>(logger,
            loaderServiceFactories,
            treeItemLoaderFactories,
            categoryItemHandlerFactories,
            loggerFactory);
        });
    }
}