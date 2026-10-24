using Import.Catalog.Product.Interfaces;
using Import.Catalog.Product.Service;
using Import.Factory.Interfaces;
using Import.KeyHash;
using Import.ServiceState;
using Import.Settings.Interfaces;
using Import.Settings.Json;
using Microsoft.Extensions.Logging;
using Moq;

namespace Import.Catalog.Product.Service.Stateful.Factory.Test;

public class ProductStatefulServiceFactoryTest
{
    [Fact]
    public void TestCreateProductStatefulServiceWhenSettingsAreValid()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlers = new List<IProductItemHandler>
        {
            new TestProductItemHandler(),
        };

        var categoryPagingList = new List<ICategoryPaging<TestCategoryProducts>>
        {
            new TestCategoryPaging(),
        };

        var categorySerializerList = new List<IItemSerializer<TestCategoryProducts>>
        {
            new TestItemSerializer(),
        };

        var keyHasherList = new List<IKeyHasher>
        {
            new TestKeyHasher(),
        };

        var serviceStateRepositoryFactoryList = new List<IServiceStateRepositoryFactory>
        {
            new TestServiceStateRepositoryFactory(),
        };

        var serviceFactory = new TestProductStatefulServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = CreateImportSettingsMock();

        var service = serviceFactory.Create("TestService", importSourceMock.Object, importSettingsMock);

        Assert.NotNull(service);
    }

    [Fact]
    public void TestCreateProductStatefulServiceThrowsExceptionWhenProductServiceOptionsNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlers = new List<IProductItemHandler>
        {
            new TestProductItemHandler(),
        };

        var categoryPagingList = new List<ICategoryPaging<TestCategoryProducts>>
        {
            new TestCategoryPaging(),
        };

        var categorySerializerList = new List<IItemSerializer<TestCategoryProducts>>
        {
            new TestItemSerializer(),
        };

        var keyHasherList = new List<IKeyHasher>
        {
            new TestKeyHasher(),
        };

        var serviceStateRepositoryFactoryList = new List<IServiceStateRepositoryFactory>
        {
            new TestServiceStateRepositoryFactory(),
        };

        var serviceFactory = new TestProductStatefulServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = new Mock<IImportSettings>();
        importSettingsMock.Setup(x => x.TryGetService<ProductServiceOptions>(It.Is<string>(s => s == "ProductServiceOptions"), out It.Ref<ProductServiceOptions>.IsAny))
            .Returns(false);

        var exception = Assert.Throws<Exception>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsMock.Object));
        Assert.Contains("productServiceOptions", exception.Message);
    }

    [Fact]
    public void TestCreateProductStatefulServiceThrowsExceptionWhenProductPathFormatTypeNotSet()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlers = new List<IProductItemHandler>
        {
            new TestProductItemHandler(),
        };

        var categoryPagingList = new List<ICategoryPaging<TestCategoryProducts>>
        {
            new TestCategoryPaging(),
        };

        var categorySerializerList = new List<IItemSerializer<TestCategoryProducts>>
        {
            new TestItemSerializer(),
        };

        var keyHasherList = new List<IKeyHasher>
        {
            new TestKeyHasher(),
        };

        var serviceStateRepositoryFactoryList = new List<IServiceStateRepositoryFactory>
        {
            new TestServiceStateRepositoryFactory(),
        };

        var serviceFactory = new TestProductStatefulServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = new Mock<IImportSettings>();
        var productServiceOptions = new ProductServiceOptions(); // ProductPathFormatType is default
        importSettingsMock.Setup(x => x.TryGetService<ProductServiceOptions>(It.Is<string>(s => s == "ProductServiceOptions"), out productServiceOptions))
            .Returns(true);

        var exception = Assert.Throws<Exception>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsMock.Object));
        Assert.Contains("ProductPathFormatType", exception.Message);
    }

    [Fact]
    public void TestCreateProductStatefulServiceThrowsExceptionWhenCategoryPathFormatTypeNotSet()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlers = new List<IProductItemHandler>
        {
            new TestProductItemHandler(),
        };

        var categoryPagingList = new List<ICategoryPaging<TestCategoryProducts>>
        {
            new TestCategoryPaging(),
        };

        var categorySerializerList = new List<IItemSerializer<TestCategoryProducts>>
        {
            new TestItemSerializer(),
        };

        var keyHasherList = new List<IKeyHasher>
        {
            new TestKeyHasher(),
        };

        var serviceStateRepositoryFactoryList = new List<IServiceStateRepositoryFactory>
        {
            new TestServiceStateRepositoryFactory(),
        };

        var serviceFactory = new TestProductStatefulServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = new Mock<IImportSettings>();
        var productServiceOptions = new ProductServiceOptions
        {
            ProductPathFormatType = PathFormatType.ItemId
        };
        importSettingsMock.Setup(x => x.TryGetService<ProductServiceOptions>(It.Is<string>(s => s == "ProductServiceOptions"), out productServiceOptions))
            .Returns(true);

        var exception = Assert.Throws<Exception>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsMock.Object));
        Assert.Contains("CategoryPathFormatType", exception.Message);
    }

    [Fact]
    public void TestCreateProductStatefulServiceThrowsInvalidDataExceptionWhenLoaderFactoryNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>();

        var productItemHandlers = new List<IProductItemHandler>
        {
            new TestProductItemHandler(),
        };

        var categoryPagingList = new List<ICategoryPaging<TestCategoryProducts>>
        {
            new TestCategoryPaging(),
        };

        var categorySerializerList = new List<IItemSerializer<TestCategoryProducts>>
        {
            new TestItemSerializer(),
        };

        var keyHasherList = new List<IKeyHasher>
        {
            new TestKeyHasher(),
        };

        var serviceStateRepositoryFactoryList = new List<IServiceStateRepositoryFactory>
        {
            new TestServiceStateRepositoryFactory(),
        };

        var serviceFactory = new TestProductStatefulServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsMock));
        Assert.Contains("Loader", exception.Message);
    }

    [Fact]
    public void TestCreateProductStatefulServiceThrowsInvalidDataExceptionWhenProductItemHandlerNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlers = new List<IProductItemHandler>();

        var categoryPagingList = new List<ICategoryPaging<TestCategoryProducts>>
        {
            new TestCategoryPaging(),
        };

        var categorySerializerList = new List<IItemSerializer<TestCategoryProducts>>
        {
            new TestItemSerializer(),
        };

        var keyHasherList = new List<IKeyHasher>
        {
            new TestKeyHasher(),
        };

        var serviceStateRepositoryFactoryList = new List<IServiceStateRepositoryFactory>
        {
            new TestServiceStateRepositoryFactory(),
        };

        var serviceFactory = new TestProductStatefulServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsMock));
        Assert.Contains("ProductItemHandler", exception.Message);
    }

    [Fact]
    public void TestCreateProductStatefulServiceThrowsInvalidDataExceptionWhenCategoryPagingNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlers = new List<IProductItemHandler>
        {
            new TestProductItemHandler(),
        };

        var categoryPagingList = new List<ICategoryPaging<TestCategoryProducts>>();

        var categorySerializerList = new List<IItemSerializer<TestCategoryProducts>>
        {
            new TestItemSerializer(),
        };

        var keyHasherList = new List<IKeyHasher>
        {
            new TestKeyHasher(),
        };

        var serviceStateRepositoryFactoryList = new List<IServiceStateRepositoryFactory>
        {
            new TestServiceStateRepositoryFactory(),
        };

        var serviceFactory = new TestProductStatefulServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsMock));
        Assert.Contains("CategoryPaging", exception.Message);
    }

    [Fact]
    public void TestCreateProductStatefulServiceThrowsInvalidDataExceptionWhenCategorySerializerNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlers = new List<IProductItemHandler>
        {
            new TestProductItemHandler(),
        };

        var categoryPagingList = new List<ICategoryPaging<TestCategoryProducts>>
        {
            new TestCategoryPaging(),
        };

        var categorySerializerList = new List<IItemSerializer<TestCategoryProducts>>();

        var keyHasherList = new List<IKeyHasher>
        {
            new TestKeyHasher(),
        };

        var serviceStateRepositoryFactoryList = new List<IServiceStateRepositoryFactory>
        {
            new TestServiceStateRepositoryFactory(),
        };

        var serviceFactory = new TestProductStatefulServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsMock));
        Assert.Contains("TestItemSerializer", exception.Message);
    }

    [Fact]
    public void TestCreateProductStatefulServiceThrowsInvalidDataExceptionWhenKeyHasherNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlers = new List<IProductItemHandler>
        {
            new TestProductItemHandler(),
        };

        var categoryPagingList = new List<ICategoryPaging<TestCategoryProducts>>
        {
            new TestCategoryPaging(),
        };

        var categorySerializerList = new List<IItemSerializer<TestCategoryProducts>>
        {
            new TestItemSerializer(),
        };

        var keyHasherList = new List<IKeyHasher>();

        var serviceStateRepositoryFactoryList = new List<IServiceStateRepositoryFactory>
        {
            new TestServiceStateRepositoryFactory(),
        };

        var serviceFactory = new TestProductStatefulServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsMock));
        Assert.Contains("TestKeyHasher", exception.Message);
    }

    [Fact]
    public void TestCreateProductStatefulServiceThrowsInvalidDataExceptionWhenStateRepositoryFactoryNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlers = new List<IProductItemHandler>
        {
            new TestProductItemHandler(),
        };

        var categoryPagingList = new List<ICategoryPaging<TestCategoryProducts>>
        {
            new TestCategoryPaging(),
        };

        var categorySerializerList = new List<IItemSerializer<TestCategoryProducts>>
        {
            new TestItemSerializer(),
        };

        var keyHasherList = new List<IKeyHasher>
        {
            new TestKeyHasher(),
        };

        var serviceStateRepositoryFactoryList = new List<IServiceStateRepositoryFactory>();

        var serviceFactory = new TestProductStatefulServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsMock));
        Assert.Contains("TestServiceStateRepositoryFactory", exception.Message);
    }

    private static IImportSettings CreateImportSettingsMock()
    {
        var importSettingsMock = new Mock<IImportSettings>();

        string productSourceUrl = "http://TestSource";
        importSettingsMock.Setup(x => x.TryGetString(It.Is<string>(s => s == "ProductSourceUrl"), out productSourceUrl))
            .Returns(true);

        var productServiceOptions = new ProductServiceOptions
        {
            ProductPathFormatType = PathFormatType.ItemId,
            CategoryPathFormatType = PathFormatType.Path
        };
        importSettingsMock.Setup(x => x.TryGetService<ProductServiceOptions>(It.Is<string>(s => s == "ProductServiceOptions"), out productServiceOptions))
            .Returns(true);

        IServiceSettings loaderServiceSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestProductLoaderService"
        };
        importSettingsMock.Setup(x => x.TryGetServiceSettings(It.Is<string>(s => s == "Loader"), out loaderServiceSettings))
            .Returns(true);

        IServiceSettings productItemHandlerSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestProductItemHandler"
        };
        importSettingsMock.Setup(x => x.TryGetServiceSettings(It.Is<string>(s => s == "ProductItemHandler"), out productItemHandlerSettings))
            .Returns(true);

        IServiceSettings categoryPagingSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestCategoryPaging"
        };
        importSettingsMock.Setup(x => x.TryGetServiceSettings(It.Is<string>(s => s == "CategoryPaging"), out categoryPagingSettings))
            .Returns(true);

        IServiceSettings categorySerializerSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestItemSerializer"
        };
        importSettingsMock.Setup(x => x.TryGetServiceSettings(It.Is<string>(s => s == "CategorySerializer"), out categorySerializerSettings))
            .Returns(true);

        IServiceSettings keyHasherSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestKeyHasher"
        };
        importSettingsMock.Setup(x => x.TryGetServiceSettings(It.Is<string>(s => s == "KeyHasher"), out keyHasherSettings))
            .Returns(true);

        IServiceSettings stateRepositoryFactorySettings = new ServiceSettings
        {
            ImplementationTypeName = "TestServiceStateRepositoryFactory"
        };
        importSettingsMock.Setup(x => x.TryGetServiceSettings(It.Is<string>(s => s == "StateRepositoryFactory"), out stateRepositoryFactorySettings))
            .Returns(true);

        var catalogCategories = new List<IProductCatalogCategory>
        {
            new TestProductCatalogCategory { Category = "Test Category", ItemId = 1, Path = "/test-category" }
        };
        importSettingsMock.Setup(x => x.TryGet(It.Is<string>(s => s == "CatalogCategories"), out catalogCategories))
            .Returns(true);

        string productPathFormat = "products/{0}";
        importSettingsMock.Setup(x => x.TryGetString(It.Is<string>(s => s == "ProductPathFormat"), out productPathFormat))
            .Returns(true);

        string categoryPathFormat = "categories/{0}";
        importSettingsMock.Setup(x => x.TryGetString(It.Is<string>(s => s == "CategoryPathFormat"), out categoryPathFormat))
            .Returns(true);

        return importSettingsMock.Object;
    }
}
