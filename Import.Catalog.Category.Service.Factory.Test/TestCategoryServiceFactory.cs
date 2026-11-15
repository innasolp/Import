using System.Collections;
using Import.Catalog.Category.Factory.Interfaces;
using Import.Catalog.Category.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory.Test;

internal class TestLoaderService1 : ILoaderService
{
    public string Name => "TestLoaderService1";
    public bool IsStarted => true;

    public Task Close(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    public Task<object> GetData(string host, CancellationToken cancellationToken = default) => Task.FromResult<object>(null);
    public Task<Stream> Load(string url, object? data, CancellationToken cancellationToken = default) => Task.FromResult<Stream>(null);
    public Task Reset(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task Start(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task UpdateData(string url, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal class TestLoaderService2 : ILoaderService
{
    public string Name => "TestLoaderService2";
    public bool IsStarted => true;

    public Task Close(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    public Task<object> GetData(string host, CancellationToken cancellationToken = default) => Task.FromResult<object>(null);
    public Task<Stream> Load(string url, object? data, CancellationToken cancellationToken = default) => Task.FromResult<Stream>(null);
    public Task Reset(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task Start(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task UpdateData(string url, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal class TestLoaderService1Factory : ILoaderServiceFactory
{
    public Type GetServiceType() => typeof(TestLoaderService1);
    public ILoaderService Create(string name, IImportSettingsProvider importSettingsProvider, IImportSettings importSettings)
     => new TestLoaderService1();
}

internal class TestLoaderService2Factory : ILoaderServiceFactory
{
    public Type GetServiceType() => typeof(TestLoaderService2);
    public ILoaderService Create(string name, IImportSettingsProvider importSettingsProvider,
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

internal class TestCategoryItemHandler1Factory<TCategory> : ICategoryItemHandlerFactory<TCategory>
    where TCategory : ICategory
{
    public Type GetServiceType() => typeof(TestCategoryItemHandler1<TCategory>);
    public ICategoryItemHandler<TCategory> Create(IImportSettingsProvider importSettingsProvider, IImportSettings importSettings)
        => new TestCategoryItemHandler1<TCategory>();
}

internal class TestCategoryItemHandler2Factory<TCategory> : ICategoryItemHandlerFactory<TCategory>
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

public class TestCategoryService<TTreeItem, TCategory>(
    ILogger logger,
    string name,
    ILoaderService loader,
    string categorySourceUrl,
    string sourceName,
    string url,
    IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
    ICategoryItemHandler<TCategory> itemHandler,
    object? categoryLoadData = null)
    : CategoryService<TTreeItem, TCategory>(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadStages, itemHandler, categoryLoadData)
    where TTreeItem : TreeItem.TreeItem, new()
    where TCategory : ICategory, new()
{
    public override string Name { get; } = name;
}

public class TestCategoryServiceFactory<TTreeItem, TCategory>(
    ILogger<TestCategoryService<TTreeItem, TCategory>> logger,
    IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
    IEnumerable<ITreeItemLoaderFactory<TTreeItem>> treeItemLoaderFactories,
    IEnumerable<ICategoryItemHandlerFactory<TCategory>> categoryItemHandlerFactories,
    IImportServiceLogFactory? logFactory = null)
    : CategoryServiceFactory<TestCategoryService<TTreeItem, TCategory>, TTreeItem, TCategory>(logger, loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlerFactories, logFactory)
    where TTreeItem : TreeItem.TreeItem, new()
    where TCategory : ICategory, new()
{
    public override Type GetServiceType() => typeof(TestCategoryService<TTreeItem, TCategory>);

    protected override TestCategoryService<TTreeItem, TCategory> Create(
        ILogger<TestCategoryService<TTreeItem, TCategory>> logger,
        string name,
        ILoaderService loader,
        string categorySourceUrl,
        string sourceName,
        string url,
        IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
        ICategoryItemHandler<TCategory> categoryItemHandler,
        IImportSettingsProvider importSettingsProvider,
        IImportSettings importSettings,
        object? categoryLoadData = null)
    {
        return new TestCategoryService<TTreeItem, TCategory>(
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
