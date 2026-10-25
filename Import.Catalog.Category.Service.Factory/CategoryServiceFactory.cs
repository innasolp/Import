using Import.Catalog.Category.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

public abstract class CategoryServiceFactory<TTreeItem>(ILogger logger,
    IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
    IEnumerable<ITreeItemLoaderFactory<TTreeItem>> treeItemLoaderFactories,
    IEnumerable<ICategoryItemHandler<TTreeItem>> categoryItemHandlers,
    IImportServiceLogFactory? logFactory = null)
    : IImportServiceFactory
    where TTreeItem : TreeItem.TreeItem, new()
{
    private readonly IEnumerable<ITreeItemLoaderFactory<TTreeItem>> _treeItemLoaderFactories = treeItemLoaderFactories;

    public IService Create(string name, IImportSource importSource, IImportSettings importSettings)
    {
        if (!importSettings.TryGetString("CategorySourceUrl", out var categorySourceUrl))
            throw new Exception($"Can't find {nameof(categorySourceUrl)}.");

        importSettings.TryGetJson("CategoryLoadData", out var categoryLoadData);

        if (!importSettings.TryGetService<LoadStageSettings[]>("LoadStages", out var loadStagesSettings))
            throw new Exception($"Can't find {nameof(loadStagesSettings)}.");

        var treeItemLoaders = new List<ITreeItemLoader<TTreeItem>>(loadStagesSettings.Length);
        foreach (var stageSettings in loadStagesSettings)
        {
            if (string.IsNullOrEmpty(stageSettings.TypeName))
                throw new InvalidDataException($"No tree item loader factory for type {stageSettings.TypeName} for service {name}.");

            var matchingTreeItemFactories = _treeItemLoaderFactories
                .Where(f => f.GetType().Name.Contains(stageSettings.TypeName, StringComparison.InvariantCultureIgnoreCase))
                .Take(2)
                .ToArray();
            if (matchingTreeItemFactories.Length != 1)
                throw new InvalidDataException($"Expected exactly one tree item loader factory for type {stageSettings.TypeName} for service {name}.");

            var treeItemFactory = matchingTreeItemFactories[0];
            treeItemLoaders.Add(treeItemFactory.CreateTreeItemLoader(stageSettings.OptionsJson));
        }

        var serviceLogger = logFactory?.GetLogger(logger, name, importSource, importSettings) ?? logger;

        var loaderFactory = loaderServiceFactories.GetFactoryBySettings(importSettings, "Loader", name);
        var loader = loaderFactory.Create(name, importSettings);

        var categoryItemHandler = categoryItemHandlers.GetItemBySettings(importSettings, "CategoryItemHandler", name);

        return Create(serviceLogger, name, loader, categorySourceUrl, importSource.Name, importSource.Url
               , treeItemLoaders, categoryItemHandler, importSettings, categoryLoadData);
    }

    public abstract Type GetServiceType();
    protected abstract CategoryService<TTreeItem> Create(ILogger logger,
            string name,
            ILoaderService loader,
            string categorySourceUrl,
            string sourceName,
            string url,
            IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
            ICategoryItemHandler<TTreeItem> categoryItemHandler,
            IImportSettings importSettings,
            object? categoryLoadData = null);
}