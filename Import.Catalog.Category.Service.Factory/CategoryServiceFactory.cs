using Import.Catalog.Category.Factory.Interfaces;
using Import.Catalog.Category.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

public abstract class CategoryServiceFactory<TCategoryService, TTreeItem, TCategory>(ILogger<TCategoryService> logger,
    IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
    IEnumerable<ITreeItemLoaderFactory<TTreeItem>> treeItemLoaderFactories,
    IEnumerable<ICategoryItemHandlerFactory<TCategory>> categoryItemHandlerFactories,
    IImportServiceLogFactory? logFactory = null)
    : IServiceFactory
    where TTreeItem : TreeItem.TreeItem, new()
    where TCategory : ICategory, new()
    where TCategoryService : CategoryService<TTreeItem, TCategory>
{
    private const string CategorySourceUrl = "CategorySourceUrl";
    private const string CategoryLoadData = "CategoryLoadData";
    private const string LoadStages = "LoadStages";
    private const string Loader = "Loader";
    private const string CategoryItemHandler = "CategoryItemHandler";
    private readonly IEnumerable<ITreeItemLoaderFactory<TTreeItem>> _treeItemLoaderFactories = treeItemLoaderFactories;

    public IService Create(string name,
    IImportSource importSource,
    IImportSettingsProvider importSettingsProvider,
      IImportSettings importSettings)
    {
        if (!importSettingsProvider.TryGetString(importSettings, CategorySourceUrl, out var categorySourceUrl))
            throw new Exception($"Can't find {nameof(categorySourceUrl)}.");

        importSettingsProvider.TryGetJson(importSettings, CategoryLoadData, out var categoryLoadData);

        if (!importSettingsProvider.TryGetService<LoadStageSettings[]>(importSettings, LoadStages, out var loadStagesSettings))
            throw new Exception($"Can't find {nameof(loadStagesSettings)}.");

        var treeItemLoaders = new List<ITreeItemLoader<TTreeItem>>(loadStagesSettings.Length);
        foreach (var stageSettings in loadStagesSettings)
        {
            if (string.IsNullOrEmpty(stageSettings.TypeName))
                throw new InvalidDataException($"No tree item loader factory for type {stageSettings.TypeName} for service {name}.");

            var treeItemFactory = _treeItemLoaderFactories.GetFactoryByImplementationName(stageSettings.TypeName, name);
            if (treeItemFactory == default)
                throw new InvalidDataException($"Tree item loader factory for type {stageSettings.TypeName} for service {name} not found.");

            treeItemLoaders.Add(treeItemFactory.CreateTreeItemLoader(stageSettings.OptionsJson));
        }

        var serviceLogger = logFactory?.GetLogger(logger, name, importSource, importSettings) ?? logger;

        var loaderFactory = loaderServiceFactories.GetFactoryBySettings(importSettingsProvider, importSettings, Loader, name);
        var loader = loaderFactory.Create(name, importSettingsProvider, importSettings);

        var categoryItemHandlerFactory = categoryItemHandlerFactories.GetFactoryBySettings(importSettingsProvider, importSettings, CategoryItemHandler, name);
        var categoryItemHandler = categoryItemHandlerFactory.Create(importSettingsProvider, importSettings);

        return Create(serviceLogger, name, loader, categorySourceUrl, importSource.Name, importSource.Url
               , treeItemLoaders, categoryItemHandler, importSettingsProvider, importSettings, categoryLoadData);
    }

    public abstract Type GetServiceType();

    protected abstract TCategoryService Create(ILogger<TCategoryService> logger,
            string name,
            ILoaderService loader,
            string categorySourceUrl,
            string sourceName,
            string url,
            IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
            ICategoryItemHandler<TCategory> categoryItemHandler,
            IImportSettingsProvider importSettingsProvider,
            IImportSettings importSettings,
            object? categoryLoadData = null);
}