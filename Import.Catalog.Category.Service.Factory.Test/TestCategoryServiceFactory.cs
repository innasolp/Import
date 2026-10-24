using System.Collections;
using Import.Catalog.Category.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory.Test;

internal class TestLoaderService1 : ILoaderService
{
    public string Name => throw new NotImplementedException();

    public bool IsStarted => throw new NotImplementedException();

    public Task Close(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

    public Task<object> GetData(string host, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> Load(string url, object? data, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Reset(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Start(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateData(string url, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

internal class TestLoaderService2 : ILoaderService
{
    public string Name => throw new NotImplementedException();

    public bool IsStarted => throw new NotImplementedException();

    public Task Close(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

    public Task<object> GetData(string host, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> Load(string url, object? data, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Reset(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Start(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateData(string url, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

internal class TestLoaderService1Factory : ILoaderServiceFactory
{
    public Type GetServiceType()
    {
        return typeof(TestLoaderService1);
    }

    public ILoaderService Create(string name, IImportSettings importSettings)
    {
        throw new NotImplementedException();
    }
}

internal class TestLoaderService2Factory : ILoaderServiceFactory
{
    public ILoaderService Create(string name, IImportSettings importSettings)
    {
        return new TestLoaderService2();
    }

    public Type GetServiceType()
    {
        return typeof(TestLoaderService2);
    }
}

internal class TestCategoryItemHandler1<TTreeItem> : ICategoryItemHandler<TTreeItem>
{
    public event AsyncEventHandler<ItemProcessEventArgs<IImportCategory<TTreeItem>, ResultStatus>> ItemProcessed;

    public Task<ResultStatus> HandleItem(IImportCategory<TTreeItem> item, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

internal class TestCategoryItemHandler2<TTreeItem> : ICategoryItemHandler<TTreeItem>
{
    public event AsyncEventHandler<ItemProcessEventArgs<IImportCategory<TTreeItem>, ResultStatus>> ItemProcessed;

    public Task<ResultStatus> HandleItem(IImportCategory<TTreeItem> item, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

internal class TestCategory : TreeItem.TreeItem
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

internal class TestCategoryService<TTreeItem>(ILogger logger,
string name,
 ILoaderService loader,
  string categorySourceUrl,
  string sourceName,
   string url,
   IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
   ICategoryItemHandler<TTreeItem> itemHandler,
   object? categoryLoadData = null) : CategoryService<TTreeItem>(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadStages, itemHandler, categoryLoadData)
   where TTreeItem : TreeItem.TreeItem, new()
{
}

public class TestCategoryServiceFactory<TTreeItem>(ILogger logger,
IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
IEnumerable<ITreeItemLoaderFactory<TTreeItem>> treeItemLoaderFactories,
 IEnumerable<ICategoryItemHandler<TTreeItem>> categoryItemHandlers,
  IImportServiceLogFactory? logFactory = null)
  : CategoryServiceFactory<TTreeItem>(logger, loaderServiceFactories, treeItemLoaderFactories, categoryItemHandlers, logFactory)
   where TTreeItem : TreeItem.TreeItem, new()
{
    public override Type GetServiceType()
    {
        return typeof(TestCategoryService<TTreeItem>);
    }

    protected override CategoryService<TTreeItem> Create(ILogger logger,
     string name,
      ILoaderService loader,
      string categorySourceUrl,
       string sourceName,
       string url,
       IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
       ICategoryItemHandler<TTreeItem> categoryItemHandler,
       IImportSettings importSettings,
       object? categoryLoadData = null)
    {
        return new TestCategoryService<TTreeItem>(logger, name, loader, categorySourceUrl, name, url
               , categoryLoadStages, categoryItemHandler, categoryLoadData);
    }
}
