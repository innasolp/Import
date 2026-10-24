using Import.Catalog.Category.Interfaces;
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
        return services.AddScoped<IImportServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<CategoryService<TTreeItem>>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            var loaderServiceFactories = sd.GetServices<ILoaderServiceFactory>();
            var treeItemLoaderFactories = sd.GetServices<ITreeItemLoaderFactory<TTreeItem>>();
            var categoryItemHandlers = sd.GetServices<ICategoryItemHandler<TTreeItem>>();
            return new CategoryServiceFactoryImpl<TTreeItem>(logger,
            loaderServiceFactories,
            treeItemLoaderFactories,
            categoryItemHandlers,
            loggerFactory);
        });
    }

    public static IServiceCollection AddCategoryService(this IServiceCollection services)
    {
        return services.AddCategoryService<Category>();
    }

    public static IServiceCollection AddCategoryTimerService<TTreeItem>(this IServiceCollection services)
    where TTreeItem : TreeItem.TreeItem, new()
    {
        return services.AddScoped<IImportServiceFactory>(sd =>
        {
            var logger = sd.GetRequiredService<ILogger<CategoryService<TTreeItem>>>();
            var loggerFactory = sd.GetService<IImportServiceLogFactory>();
            var loaderServiceFactories = sd.GetServices<ILoaderServiceFactory>();
            var treeItemLoaderFactories = sd.GetServices<ITreeItemLoaderFactory<TTreeItem>>();
            var categoryItemHandlers = sd.GetServices<ICategoryItemHandler<TTreeItem>>();
            return new CategoryTimerServiceFactory<TTreeItem>(logger,
            loaderServiceFactories,
            treeItemLoaderFactories,
            categoryItemHandlers,
            loggerFactory);
        });
    }
}