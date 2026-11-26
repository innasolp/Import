using System.Collections;
using Import.Catalog.Category.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory.Test;

internal class TestLoaderService1 : ILoaderService<LoadData<object>>
{
    public string Name => "TestLoaderService1";
    public bool IsStarted => true;

    public Task Close(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    public Task<object> GetData(string host, CancellationToken cancellationToken = default) => Task.FromResult<object>(null);
    public Task<Stream> Load(string url, LoadData<object>? data, CancellationToken cancellationToken = default) => Task.FromResult<Stream>(null);
    public Task Reset(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task Start(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task UpdateData(string url, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal class TestLoaderService2 : ILoaderService<LoadData<object>>
{
    public string Name => "TestLoaderService2";
    public bool IsStarted => true;

    public Task Close(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    public Task<object> GetData(string host, CancellationToken cancellationToken = default) => Task.FromResult<object>(null);
    public Task<Stream> Load(string url, LoadData<object>? data, CancellationToken cancellationToken = default) => Task.FromResult<Stream>(null);
    public Task Reset(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task Start(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task UpdateData(string url, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal class TestLoaderService1Factory : ILoaderServiceFactory<LoadData<object>>
{
    public Type GetServiceType() => typeof(TestLoaderService1);
    public ILoaderService<LoadData<object>> Create(string name, IImportSettingsProvider importSettingsProvider, IImportSettings importSettings)
     => new TestLoaderService1();
}

internal class TestLoaderService2Factory : ILoaderServiceFactory<LoadData<object>>
{
    public Type GetServiceType() => typeof(TestLoaderService2);
    public ILoaderService<LoadData<object>> Create(string name, IImportSettingsProvider importSettingsProvider,
      IImportSettings importSettings) => new TestLoaderService2();
}

internal class TestCategoryItemHandler1<TCategory> : ICategoryItemHandler<TCategory>
    where TCategory : ICategory
{
    public event AsyncEventHandler<ItemProcessEventArgs<IImportCategory<TCategory>, ResultStatus>>? ItemProcessed;

    public Task<ResultStatus> HandleItem(IImportCategory<TCategory> item, CancellationToken cancellationToken = default)
        => Task.FromResult(ResultStatus.Success);
}

internal class TestCategoryItemHandler2<TCategory> : ICategoryItemHandler<TCategory>
    where TCategory : ICategory
{
    public event AsyncEventHandler<ItemProcessEventArgs<IImportCategory<TCategory>, ResultStatus>>? ItemProcessed;

    public Task<ResultStatus> HandleItem(IImportCategory<TCategory> item, CancellationToken cancellationToken = default)
        => Task.FromResult(ResultStatus.Success);
}

internal class TestCategoryItemHandler1Factory<TCategory> : IFactory<ICategoryItemHandler<TCategory>>
    where TCategory : ICategory
{
    public Type GetServiceType() => typeof(TestCategoryItemHandler1<TCategory>);
    public ICategoryItemHandler<TCategory> Create(IImportSettingsProvider importSettingsProvider, IImportSettings importSettings)
        => new TestCategoryItemHandler1<TCategory>();
}

internal class TestCategoryItemHandler2Factory<TCategory> : IFactory<ICategoryItemHandler<TCategory>>
    where TCategory : ICategory
{
    public Type GetServiceType() => typeof(TestCategoryItemHandler2<TCategory>);
    public ICategoryItemHandler<TCategory> Create(IImportSettingsProvider importSettingsProvider, IImportSettings importSettings)
        => new TestCategoryItemHandler2<TCategory>();
}

public class TestCategory : TreeItem.TreeItem
{
    private readonly List<TestCategory> _children = [];
    protected override IList Children => _children;
    private TestCategory? _parent;
    protected override TreeItem.TreeItem? ItemParent { get => _parent; set => _parent = value as TestCategory; }
    public string? Description { get; private set; }

    protected override void SetProperties<TElement>(TElement element, IReadOnlyDictionary<string, PropertyPath> propertyPaths, IElementHelper<TElement> elementHelper)
    {
        if (propertyPaths.TryGetValue(nameof(Description), out var descriptionPath)
            && TryGetProperty(element, descriptionPath, elementHelper.GetString, elementHelper, out var description))
        {
            Description = description;
        }
    }
}

public class TestCategoryService<TLoadOptions, TTreeItem, TCategory>(
    ILogger logger,
    string name,
    ILoaderService<LoadData<TLoadOptions>> loader,
    string categorySourceUrl,
    string sourceName,
    string url,
    IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
    ICategoryItemHandler<TCategory> itemHandler,
    TLoadOptions? categoryLoadData = null)
    : CategoryService<TLoadOptions, TTreeItem, TCategory>(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadStages, itemHandler, categoryLoadData)
    where TTreeItem : TreeItem.TreeItem, new()
    where TCategory : ICategory, new()
    where TLoadOptions : class
{
    public override string Name { get; } = name;
}

public class TestCategoryServiceFactory<TLoadOptions, TTreeItem, TCategory>(
    ILogger<TestCategoryService<TLoadOptions, TTreeItem, TCategory>> logger,
    IEnumerable<ILoaderServiceFactory<LoadData<TLoadOptions>>> loaderServiceFactories,
    IEnumerable<ITreeItemLoaderFactory<TTreeItem>> treeItemLoaderFactories,
    IEnumerable<IFactory<ICategoryItemHandler<TCategory>>> categoryItemHandlerFactories,
    IImportServiceLogFactory? logFactory = null)
    : CategoryServiceFactory<TLoadOptions, TestCategoryService<TLoadOptions, TTreeItem, TCategory>, TTreeItem, TCategory>(logger, loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlerFactories, logFactory)
    where TTreeItem : TreeItem.TreeItem, new()
    where TCategory : ICategory, new()
    where TLoadOptions : class
{
    public override Type GetServiceType() => typeof(TestCategoryService<TLoadOptions, TTreeItem, TCategory>);

    protected override TestCategoryService<TLoadOptions, TTreeItem, TCategory> Create(
        ILogger<TestCategoryService<TLoadOptions, TTreeItem, TCategory>> logger,
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
        return new TestCategoryService<TLoadOptions, TTreeItem, TCategory>(
            logger,
            name,
            loader,
            categorySourceUrl,
            sourceName,
            url,
            categoryLoadStages,
            categoryItemHandler,
            categoryLoadData);
    }
}