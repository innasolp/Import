using Import.Catalog.Product.Factory.Interfaces;
using Import.Catalog.Product.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
using Import.KeyHash;
using Import.ServiceState;
using Import.ServiceState.InMemory;
using Import.Settings.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Stateful.Factory.Test;

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

public class TestProductCatalogCategory : IProductCatalogCategory
{
    public string Category { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public string Path { get; set; } = string.Empty;
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

internal class TestKeyHasher : IKeyHasher
{
    public byte[] Hash<T>(T key)
    {
        var str = key?.ToString() ?? string.Empty;
        return System.Text.Encoding.UTF8.GetBytes(str);
    }
}

internal class TestServiceStateRepositoryFactory : IServiceStateRepositoryFactory
{
    public IServiceStateRepository Create(params object[] parameters)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        return new ServiceStateInMemoryRepository(cache);
    }
}


public class TestProductStatefulService(
    ILogger<TestProductStatefulService> logger,
    string name,
    ILoaderService loader,
    string url,
    IEnumerable<IProductCatalogCategory> catalogCategories,
    IProductItemHandler productItemHandler,
    string productPathFormat,
    string categoryPathFormat,
    string sourceName,
    ICategoryPaging<TestCategoryProducts> categoryPaging,
    IItemSerializer<TestCategoryProducts> categorySerializer,
    ProductServiceOptions productServiceOptions,
    IKeyHasher keyHasher,
    IServiceStateRepository serviceStateRepository)
    : ProductStatefulService<TestCategoryProducts, TestProductItem>(
        logger, loader, url, catalogCategories, productItemHandler, productPathFormat, categoryPathFormat, sourceName,
        categoryPaging, categorySerializer, productServiceOptions, keyHasher, serviceStateRepository)
{
    public override string Name { get; } = name;
    protected override string PreparePath(string path) => path;
}

public class TestProductStatefulServiceFactory(
    ILogger<TestProductStatefulService> logger,
    IEnumerable<ILoaderServiceFactory> loaderServiceFactories,
    IEnumerable<IProductItemHandlerFactory> productItemHandlerFactories,
    IEnumerable<ICategoryPaging<TestCategoryProducts>> categoryPagingList,
    IEnumerable<IItemSerializer<TestCategoryProducts>> categorySerializerList,
    IEnumerable<IKeyHasher> keyHasherList,
    IEnumerable<IServiceStateRepositoryFactory> serviceStateRepositoryFactoryList,
    IImportServiceLogFactory? logFactory = null)
    : ProductStatefulServiceFactory<TestProductStatefulService, TestCategoryProducts, TestProductItem, TestProductCatalogCategory, ProductServiceOptions>(
        logger, loaderServiceFactories, productItemHandlerFactories, categoryPagingList, categorySerializerList,
        keyHasherList, serviceStateRepositoryFactoryList, logFactory)
{
    public override Type GetServiceType() => typeof(TestProductStatefulService);

    protected override TestProductStatefulService Create(IImportSettingsProvider importSettingsProvider, IImportSettings importSettings,
     ILogger<TestProductStatefulService> logger,
     string name,
     ILoaderService loader,
      IEnumerable<IProductCatalogCategory> catalogCategories,
       IProductItemHandler productItemHandler,
       string productPathFormat,
       string categoryPathFormat,
        string sourceName,
         string url,
          ICategoryPaging<TestCategoryProducts> categoryPaging,
          IItemSerializer<TestCategoryProducts> categorySerializer,
           ProductServiceOptions productServiceOptions,
            IKeyHasher keyHasher,
            IServiceStateRepository serviceStateRepository)
    {
        return new TestProductStatefulService(
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
            productServiceOptions,
            keyHasher,
            serviceStateRepository);
    }
}
