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
    public ILoaderService Create(string name, IImportSettings importSettings) => new TestLoaderService1();
}

internal class TestLoaderService2Factory : ILoaderServiceFactory
{
    public Type GetServiceType() => typeof(TestLoaderService2);
    public ILoaderService Create(string name, IImportSettings importSettings) => new TestLoaderService2();
}

internal class TestCategoryItemHandler1<TTreeItem> : ICategoryItemHandler<TTreeItem>
{
    public event AsyncEventHandler<ItemProcessEventArgs<IImportCategory<TTreeItem>, ResultStatus>>? ItemProcessed;

    public Task<ResultStatus> HandleItem(IImportCategory<TTreeItem> item, CancellationToken cancellationToken = default)
        => Task.FromResult(ResultStatus.Success);
}

internal class TestCategoryItemHandler2<TTreeItem> : ICategoryItemHandler<TTreeItem>
{
    public event AsyncEventHandler<ItemProcessEventArgs<IImportCategory<TTreeItem>, ResultStatus>>? ItemProcessed;

    public Task<ResultStatus> HandleItem(IImportCategory<TTreeItem> item, CancellationToken cancellationToken = default)
        => Task.FromResult(ResultStatus.Success);
}

internal class TestCategoryItemHandler1Factory<TTreeItem> : ICategoryItemHandlerFactory<TTreeItem>
{
    public Type GetServiceType() => typeof(TestCategoryItemHandler1<TTreeItem>);
    public ICategoryItemHandler<TTreeItem> Create(IImportSettingsProvider importSettingsProvider, IImportSettings importSettings)
        => new TestCategoryItemHandler1<TTreeItem>();
}

internal class TestCategoryItemHandler2Factory<TTreeItem> : ICategoryItemHandlerFactory<TTreeItem>
{
    public Type GetServiceType() => typeof(TestCategoryItemHandler2<TTreeItem>);
    public ICategoryItemHandler<TTreeItem> Create(IImportSettingsProvider importSettingsProvider, IImportSettings importSettings)
        => new TestCategoryItemHandler2<TTreeItem>();
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

public class TestCategoryService<TTreeItem>(
    ILogger logger,
    string name,
    ILoaderService loader,
    string categorySourceUrl,
    string sourceName,
    string url,
    IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
    ICategoryItemHandler<TTreeItem> itemHandler,
    object? categoryLoadData = null)
    : CategoryService<TTreeItem>(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadStages, itemHandler, categoryLoadData)
    where TTreeItem : TreeItem.TreeItem, new()
{
    public override string Name { get; } = name;
}

public class TestCategoryServiceFactory<TTreeItem>(
    ILogger<TestCategoryService<TTreeItem>> logger,
    IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
    IEnumerable<ITreeItemLoaderFactory<TTreeItem>> treeItemLoaderFactories,
    IEnumerable<ICategoryItemHandlerFactory<TTreeItem>> categoryItemHandlerFactories,
    IImportServiceLogFactory? logFactory = null)
    : CategoryServiceFactory<TestCategoryService<TTreeItem>, TTreeItem>(logger, loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlerFactories, logFactory)
    where TTreeItem : TreeItem.TreeItem, new()
{
    public override Type GetServiceType() => typeof(TestCategoryService<TTreeItem>);

    protected override TestCategoryService<TTreeItem> Create(
        ILogger<TestCategoryService<TTreeItem>> logger,
        string name,
        ILoaderService loader,
        string categorySourceUrl,
        string sourceName,
        string url,
        IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
        ICategoryItemHandler<TTreeItem> categoryItemHandler,
        IImportSettingsProvider importSettingsProvider,
        IImportSettings importSettings,
        object? categoryLoadData = null)
    {
        return new TestCategoryService<TTreeItem>(
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
