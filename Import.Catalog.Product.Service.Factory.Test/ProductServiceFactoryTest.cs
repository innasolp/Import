using Import.Catalog.Product.Interfaces;
using Import.Factory.Interfaces;
using Import.Settings.Interfaces;
using Import.Settings.Json;
using Microsoft.Extensions.Logging;
using Moq;

namespace Import.Catalog.Product.Service.Factory.Test;

public class ProductServiceFactoryTest
{
    [Fact]
    public void TestCreateProductServiceWhenSettingsAreValid()
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

        var serviceFactory = new TestProductServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
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
    public void TestCreateProductServiceThrowsExceptionWhenProductServiceOptionsNotFound()
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

        var serviceFactory = new TestProductServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
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
    public void TestCreateProductServiceThrowsExceptionWhenProductPathFormatTypeNotSet()
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

        var serviceFactory = new TestProductServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
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
    public void TestCreateProductServiceThrowsExceptionWhenCategoryPathFormatTypeNotSet()
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

        var serviceFactory = new TestProductServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
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
    public void TestCreateProductServiceThrowsInvalidDataExceptionWhenLoaderFactoryNotFound()
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

        var serviceFactory = new TestProductServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
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
    public void TestCreateProductServiceThrowsInvalidDataExceptionWhenProductItemHandlerNotFound()
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

        var serviceFactory = new TestProductServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
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
    public void TestCreateProductServiceThrowsInvalidDataExceptionWhenCategoryPagingNotFound()
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

        var serviceFactory = new TestProductServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
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
    public void TestCreateProductServiceThrowsInvalidDataExceptionWhenCategorySerializerNotFound()
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

        var serviceFactory = new TestProductServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            productItemHandlers,
            categoryPagingList,
            categorySerializerList,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsMock));
        Assert.Contains("TestItemSerializer", exception.Message);
    }

    private static IImportSettings CreateImportSettingsMock()
    {
        var importSettingsMock = new Mock<IImportSettings>();

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
