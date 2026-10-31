using Import.Catalog.Product.Factory.Interfaces;
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

        var productItemHandlerFactories = new List<IProductItemHandlerFactory>
        {
            new TestProductItemHandlerFactory(),
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

        var serviceFactory = new TestProductStatefulServiceFactory(
            new Mock<ILogger<TestProductStatefulService>>().Object,
            loaderServiceFactories,
            productItemHandlerFactories,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var (importSettings, importSettingsProvider) = CreateImportSettingsMock();
        var service = serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProvider, importSettings);

        Assert.NotNull(service);
    }

    [Fact]
    public void TestCreateProductStatefulServiceThrowsExceptionWhenProductServiceOptionsNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlerFactories = new List<IProductItemHandlerFactory>
        {
            new TestProductItemHandlerFactory(),
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

        var serviceFactory = new TestProductStatefulServiceFactory(
            new Mock<ILogger<TestProductStatefulService>>().Object,
            loaderServiceFactories,
            productItemHandlerFactories,
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
        var importSettingsProviderMock = new Mock<IImportSettingsProvider>();
        importSettingsProviderMock.Setup(x => x.TryGetService<ProductServiceOptions>(importSettingsMock.Object, It.Is<string>(s => s == "ProductServiceOptions"), out It.Ref<ProductServiceOptions>.IsAny))
            .Returns(false);

        var exception = Assert.Throws<Exception>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProviderMock.Object, importSettingsMock.Object));
        Assert.Contains("productServiceOptions", exception.Message);
    }

    [Fact]
    public void TestCreateProductStatefulServiceThrowsInvalidDataExceptionWhenLoaderFactoryNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>();

        var productItemHandlerFactories = new List<IProductItemHandlerFactory>
        {
            new TestProductItemHandlerFactory(),
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

        var serviceFactory = new TestProductStatefulServiceFactory(
            new Mock<ILogger<TestProductStatefulService>>().Object,
            loaderServiceFactories,
            productItemHandlerFactories,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var (importSettings, importSettingsProvider) = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProvider, importSettings));
        Assert.Contains("Loader", exception.Message);
    }

    [Fact]
    public void TestCreateProductStatefulServiceThrowsInvalidDataExceptionWhenProductItemHandlerNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlerFactories = new List<IProductItemHandlerFactory>();

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

        var serviceFactory = new TestProductStatefulServiceFactory(
            new Mock<ILogger<TestProductStatefulService>>().Object,
            loaderServiceFactories,
            productItemHandlerFactories,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var (importSettings, importSettingsProvider) = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProvider, importSettings));
        Assert.Contains("ProductItemHandler", exception.Message);
    }

    [Fact]
    public void TestCreateProductStatefulServiceThrowsInvalidDataExceptionWhenCategoryPagingNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlerFactories = new List<IProductItemHandlerFactory>
        {
            new TestProductItemHandlerFactory(),
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

        var serviceFactory = new TestProductStatefulServiceFactory(
            new Mock<ILogger<TestProductStatefulService>>().Object,
            loaderServiceFactories,
            productItemHandlerFactories,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var (importSettings, importSettingsProvider) = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProvider, importSettings));
        Assert.Contains("CategoryPaging", exception.Message);
    }

    [Fact]
    public void TestCreateProductStatefulServiceThrowsInvalidDataExceptionWhenCategorySerializerNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlerFactories = new List<IProductItemHandlerFactory>
        {
            new TestProductItemHandlerFactory(),
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

        var serviceFactory = new TestProductStatefulServiceFactory(
            new Mock<ILogger<TestProductStatefulService>>().Object,
            loaderServiceFactories,
            productItemHandlerFactories,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var (importSettings, importSettingsProvider) = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProvider, importSettings));
        Assert.Contains("TestItemSerializer", exception.Message);
    }

    [Fact]
    public void TestCreateProductStatefulServiceThrowsInvalidDataExceptionWhenKeyHasherNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlerFactories = new List<IProductItemHandlerFactory>
        {
            new TestProductItemHandlerFactory(),
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

        var serviceFactory = new TestProductStatefulServiceFactory(
            new Mock<ILogger<TestProductStatefulService>>().Object,
            loaderServiceFactories,
            productItemHandlerFactories,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var (importSettings, importSettingsProvider) = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProvider, importSettings));
        Assert.Contains("TestKeyHasher", exception.Message);
    }

    [Fact]
    public void TestCreateProductStatefulServiceThrowsInvalidDataExceptionWhenStateRepositoryFactoryNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlerFactories = new List<IProductItemHandlerFactory>
        {
            new TestProductItemHandlerFactory(),
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

        var serviceFactory = new TestProductStatefulServiceFactory(
            new Mock<ILogger<TestProductStatefulService>>().Object,
            loaderServiceFactories,
            productItemHandlerFactories,
            categoryPagingList,
            categorySerializerList,
            keyHasherList,
            serviceStateRepositoryFactoryList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var (importSettings, importSettingsProvider) = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProvider, importSettings));
        Assert.Contains("TestServiceStateRepositoryFactory", exception.Message);
    }

    private static (IImportSettings, IImportSettingsProvider) CreateImportSettingsMock()
    {
        var importSettingsMock = new Mock<IImportSettings>();
        var importSettingsProviderMock = new Mock<IImportSettingsProvider>();

        string productSourceUrl = "http://TestSource";
        importSettingsProviderMock.Setup(x => x.TryGetString(importSettingsMock.Object, It.Is<string>(s => s == "ProductSourceUrl"), out productSourceUrl))
            .Returns(true);

        var productServiceOptions = new ProductServiceOptions
        {
            ProductPathFormatType = PathFormatType.ItemId,
            CategoryPathFormatType = PathFormatType.Path
        };
        importSettingsProviderMock.Setup(x => x.TryGetService<ProductServiceOptions>(importSettingsMock.Object, It.Is<string>(s => s == "ProductServiceOptions"), out productServiceOptions))
            .Returns(true);

        IServiceSettings loaderServiceSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestProductLoaderService"
        };
        importSettingsProviderMock.Setup(x => x.TryGetServiceSettings(importSettingsMock.Object, It.Is<string>(s => s == "Loader"), out loaderServiceSettings))
            .Returns(true);

        IServiceSettings productItemHandlerSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestProductItemHandler"
        };
        importSettingsProviderMock.Setup(x => x.TryGetServiceSettings(importSettingsMock.Object, It.Is<string>(s => s == "ProductItemHandler"), out productItemHandlerSettings))
            .Returns(true);

        IServiceSettings categoryPagingSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestCategoryPaging"
        };
        importSettingsProviderMock.Setup(x => x.TryGetServiceSettings(importSettingsMock.Object, It.Is<string>(s => s == "CategoryPaging"), out categoryPagingSettings))
            .Returns(true);

        IServiceSettings categorySerializerSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestItemSerializer"
        };
        importSettingsProviderMock.Setup(x => x.TryGetServiceSettings(importSettingsMock.Object, It.Is<string>(s => s == "CategorySerializer"), out categorySerializerSettings))
            .Returns(true);

        IServiceSettings keyHasherSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestKeyHasher"
        };
        importSettingsProviderMock.Setup(x => x.TryGetServiceSettings(importSettingsMock.Object, It.Is<string>(s => s == "KeyHasher"), out keyHasherSettings))
            .Returns(true);

        IServiceSettings stateRepositoryFactorySettings = new ServiceSettings
        {
            ImplementationTypeName = "TestServiceStateRepositoryFactory"
        };
        importSettingsProviderMock.Setup(x => x.TryGetServiceSettings(importSettingsMock.Object, It.Is<string>(s => s == "TestServiceStateRepositoryFactory"), out stateRepositoryFactorySettings))
            .Returns(true);

        var serviceStateSettings = new ServiceStateSettings
        {
            Provider = "TestServiceStateRepositoryFactory"
        };
        importSettingsProviderMock.Setup(x => x.TryGetService<ServiceStateSettings>(importSettingsMock.Object, It.Is<string>(s => s == "ServiceState"), out serviceStateSettings))
            .Returns(true);

        IEnumerable<TestProductCatalogCategory>? capturedCategories = new TestProductCatalogCategory[1];
        importSettingsProviderMock.Setup(x => x.TryGet<IEnumerable<TestProductCatalogCategory>>(importSettingsMock.Object, It.Is<string>(s => s == "CatalogCategories"), out capturedCategories))
            .Returns(true);

        string productPathFormat = "products/{0}";
        importSettingsProviderMock.Setup(x => x.TryGetString(importSettingsMock.Object, It.Is<string>(s => s == "ProductPathFormat"), out productPathFormat))
            .Returns(true);

        string categoryPathFormat = "categories/{0}";
        importSettingsProviderMock.Setup(x => x.TryGetString(importSettingsMock.Object, It.Is<string>(s => s == "CategoryPathFormat"), out categoryPathFormat))
            .Returns(true);

        return (importSettingsMock.Object, importSettingsProviderMock.Object);
    }
}
