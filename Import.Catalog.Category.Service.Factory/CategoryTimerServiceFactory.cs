using Import.Catalog.Category.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

public abstract class CategoryTimerServiceFactory<TLoadOptions, TCategoryTimerService, TTreeItem, TCategory>(ILogger<TCategoryTimerService> logger,
 IEnumerable<ILoaderServiceFactory<LoadData<TLoadOptions>>> loaderServiceFactories,
  IEnumerable<ITreeItemLoaderFactory<TTreeItem>> treeItemLoaderFactories,
   IEnumerable<IFactory<ICategoryItemHandler<TCategory>>> categoryItemHandlerFactories,
   IImportServiceLogFactory? logFactory = null)
    : CategoryServiceFactory<TLoadOptions, TCategoryTimerService, TTreeItem, TCategory>(logger, loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlerFactories, logFactory)
where TTreeItem : TreeItem.TreeItem, new()
where TCategory : ICategory, new()
where TLoadOptions : class
where TCategoryTimerService : CategoryTimerService<TLoadOptions, TTreeItem, TCategory>
{
    public const string Interval = "Interval";

    protected abstract TCategoryTimerService Create(ILogger<TCategoryTimerService> logger,
    string name,
    ILoaderService<LoadData<TLoadOptions>> loader,
    string categorySourceUrl,
    string sourceName,
     string url,
      IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
      ICategoryItemHandler<TCategory> categoryItemHandler,
      IImportSettingsProvider importSettingsProvider,
      IImportSettings importSettings,
       TLoadOptions? categoryLoadData = null,
       int? interval = null);

    protected override TCategoryTimerService Create(ILogger<TCategoryTimerService> logger, 
    string name, 
    ILoaderService<LoadData<TLoadOptions>> loader,
     string categorySourceUrl,
      string sourceName, 
      string url, 
      IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages, 
      ICategoryItemHandler<TCategory> categoryItemHandler,
       IImportSettingsProvider importSettingsProvider, 
       IImportSettings importSettings, 
       TLoadOptions? categoryLoadData = null)
    {
        var interval = importSettingsProvider.TryGet<int>(importSettings, Interval, out var intervalValue) && intervalValue > 0
        ? (int?)intervalValue : null;

        return Create(logger, 
        name,
         loader,
          categorySourceUrl,
           sourceName, 
           url, 
           categoryLoadStages, 
           categoryItemHandler, 
           importSettingsProvider,
            importSettings, 
            categoryLoadData, 
            interval);
    }

    public override Type GetServiceType() => typeof(TCategoryTimerService);
}