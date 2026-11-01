using Import.Catalog.Product.Factory.Interfaces;
using Import.Catalog.Product.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Factory.Test;

internal class TestProductLoaderService : ILoaderService
{
    public string Name => "TestProductLoader";

    public bool IsStarted => true;

    public Task Close(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public Task<object> GetData(string host, CancellationToken cancellationToken = default) => Task.FromResult<object>(null);

    public Task<Stream> Load(string url, object? data, CancellationToken cancellationToken = default) => Task.FromResult<Stream>(null);

    public Task Reset(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task Start(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task UpdateData(string url, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal class TestProductLoaderServiceFactory : ILoaderServiceFactory
{
    public Type GetServiceType() => typeof(TestProductLoaderService);

    public ILoaderService Create(string name, IImportSettings importSettings) => new TestProductLoaderService();
}

internal class TestProductItemHandler : IProductItemHandler
{
    public event AsyncEventHandler<ItemProcessEventArgs<IImportProduct, ResultStatus>>? ItemProcessed;

    public Task<ResultStatus> HandleItem(IImportProduct item, CancellationToken cancellationToken = default)
        => Task.FromResult(ResultStatus.Success);
}

internal class TestProductItemHandlerFactory : IProductItemHandlerFactory
{
    public Type GetServiceType() => typeof(TestProductItemHandler);

    public IProductItemHandler Create(IImportSettingsProvider importSettingsProvider, IImportSettings settings)
        => new TestProductItemHandler();
}

internal class TestProductCatalogCategory : IProductCatalogCategory
{
    public string Category { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public string Path { get; set; } = string.Empty;
}

internal class TestCategoryProductItem : ICategoryProductItem
{
    public string Id { get; set; } = string.Empty;
    public string ItemPath { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public double Price { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryItemId { get; set; }
    public string? Brand { get; set; }
}

public class TestCategoryProducts : ICategoryProducts
{
    public ICategoryProductItem[] CategoryProductItems { get; set; } = [];
    public int? TotalCount { get; set; }
}

public class TestProductItem : IProductItem
{
    public string Name { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public double Price { get; set; }
    public string Path { get; set; } = string.Empty;
    public string AbsolutePath { get; set; } = string.Empty;
    public int CategoryId { get; set; }
}

internal class TestCategoryPaging : ICategoryPaging<TestCategoryProducts>
{
    public string GetCategoryPagePath(IProductCatalogCategory productCatalogCategory, string pathFormat, PathFormatType pathFormatType, int page, TestCategoryProducts? category = null)
        => pathFormat;

    public string GetNextCategoryPagePath(IProductCatalogCategory productCatalogCategory, string urlFormat, PathFormatType urlFormatType, int page, TestCategoryProducts? category = null)
        => urlFormat;
}

internal class TestItemSerializer : IItemSerializer<TestCategoryProducts>
{
    public Task<TestCategoryProducts?> DeserializeFromStream(Stream stream, CancellationToken cancellationToken = default)
        => Task.FromResult<TestCategoryProducts?>(new TestCategoryProducts());
}

public class TestProductService<TCategory, TProductItem>(
    ILogger logger,
    string name,
    ILoaderService loader,
    string url,
    IEnumerable<IProductCatalogCategory> catalogCategories,
    IProductItemHandler productItemHandler,
    string productPathFormat,
    string categoryPathFormat,
    string sourceName,
    ICategoryPaging<TCategory> categoryPaging,
    IItemSerializer<TCategory> categorySerializer,
    ProductServiceOptions productServiceOptions)
    : ProductService<TCategory, TProductItem>(
        logger,
        loader,
        url,
        catalogCategories,
        productItemHandler,
        productPathFormat,
        categoryPathFormat,
        sourceName,
        categoryPaging,
        categorySerializer,
        productServiceOptions)
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    public override string Name { get; } = name;

    protected override string PreparePath(string path) => path;
}

public class TestProductServiceFactory<TCategory, TProductItem, TProductCatalogCategory>(
    ILogger<TestProductService<TCategory, TProductItem>> logger,
    IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
    IEnumerable<IProductItemHandlerFactory> productItemHandlerFactories,
    IEnumerable<ICategoryPaging<TCategory>> categoryPagingList,
    IEnumerable<IItemSerializer<TCategory>> categorySerializerList,
    IImportServiceLogFactory? logFactory = null)
    : ProductServiceFactory<TestProductService<TCategory, TProductItem>, TCategory, TProductItem, TProductCatalogCategory, ProductServiceOptions>(
        logger,
        loaderServiceFactories,
        productItemHandlerFactories,
        categoryPagingList,
        categorySerializerList,
        logFactory)
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
    where TProductCatalogCategory : class, IProductCatalogCategory, new()
{
    public override Type GetServiceType() => typeof(TestProductService<TCategory, TProductItem>);

    protected override TestProductService<TCategory, TProductItem> Create(IImportSettingsProvider importSettingsProvider, IImportSettings importSettings,
     ILogger<TestProductService<TCategory, TProductItem>> logger,
      string name, ILoaderService loader,
      IEnumerable<IProductCatalogCategory> catalogCategories,
       IProductItemHandler productItemHandler,
        string productPathFormat,
         string categoryPathFormat,
         string sourceName,
         string url,
          ICategoryPaging<TCategory> categoryPaging,
           IItemSerializer<TCategory> categorySerializer,
           ProductServiceOptions productServiceOptions)
    {
        return new TestProductService<TCategory, TProductItem>(
           logger,
           name,
           loader,
           url,
           catalogCategories,
           productItemHandler,
           productPathFormat,
           categoryPathFormat,
           sourceName,
           categoryPaging,
           categorySerializer,
           productServiceOptions);
    }
}