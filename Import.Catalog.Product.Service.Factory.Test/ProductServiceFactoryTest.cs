using Import.Catalog.Product.Interfaces;
using Import.Factory.Interfaces;
using Import.Interfaces;
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
        var loaderServiceFactories = new List<ILoaderServiceFactory<LoadData<object>>>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlerFactories = new List<IFactory<IProductItemHandler>>
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

        var itemPathFormatterMock = new Mock<IItemPathFormatter<ICategoryProductItem>>();
        itemPathFormatterMock.Setup(f => f.GetAbsolutePath(It.IsAny<string>(), It.IsAny<PathFormatType>(), It.IsAny<ICategoryProductItem>()))
            .Returns((string path, PathFormatType type, ICategoryProductItem item) => path);

        var itemPathFormatterFactoryMock = new Mock<IFactory<IItemPathFormatter<ICategoryProductItem>>>();
        itemPathFormatterFactoryMock.Setup(f => f.GetServiceType()).Returns(typeof(TestItemPathFormatter));
        itemPathFormatterFactoryMock.Setup(f => f.Create(It.IsAny<IImportSettingsProvider>(), It.IsAny<IImportSettings>()))
            .Returns(itemPathFormatterMock.Object);

        var serviceFactory = new TestProductServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger<TestProductService<TestCategoryProducts, TestProductItem>>>().Object,
            loaderServiceFactories,
            productItemHandlerFactories,
            categoryPagingList,
            categorySerializerList,
            [],
            [itemPathFormatterFactoryMock.Object],
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = CreateImportSettingsMock();
        var importSettingsProviderMock = CreateImportSettingsProviderMock(importSettingsMock);

        var service = serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProviderMock.Object, importSettingsMock.Object);

        Assert.NotNull(service);
        var productService = Assert.IsType<TestProductService<TestCategoryProducts, TestProductItem>>(service);
    }

    [Fact]
    public void TestCreateProductServiceThrowsExceptionWhenProductServiceOptionsNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory<LoadData<object>>>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlerFactories = new List<IFactory<IProductItemHandler>>
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

        var serviceFactory = new TestProductServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger<TestProductService<TestCategoryProducts, TestProductItem>>>().Object,
            loaderServiceFactories,
            productItemHandlerFactories,
            categoryPagingList,
            categorySerializerList,
            [],
            [],
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = new Mock<IImportSettings>();
        var importSettingsProviderMock = new Mock<IImportSettingsProvider>();
        importSettingsProviderMock.Setup(x => x.TryGetService<ProductServiceOptions<object>>(importSettingsMock.Object, "ProductServiceOptions", out It.Ref<ProductServiceOptions<object>>.IsAny))
            .Returns(false);

        var exception = Assert.Throws<Exception>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProviderMock.Object, importSettingsMock.Object));
        Assert.Contains("productServiceOptions", exception.Message);
    }

    [Fact]
    public void TestCreateProductServiceThrowsInvalidDataExceptionWhenLoaderFactoryNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory<LoadData<object>>>();

        var productItemHandlerFactories = new List<IFactory<IProductItemHandler>>
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

        var serviceFactory = new TestProductServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger<TestProductService<TestCategoryProducts, TestProductItem>>>().Object,
            loaderServiceFactories,
            productItemHandlerFactories,
            categoryPagingList,
            categorySerializerList,
            [],
            [],
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = CreateImportSettingsMock();
        var importSettingsProviderMock = CreateImportSettingsProviderMock(importSettingsMock);

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProviderMock.Object, importSettingsMock.Object));
        Assert.Contains("Loader", exception.Message);
    }

    [Fact]
    public void TestCreateProductServiceThrowsInvalidDataExceptionWhenProductItemHandlerNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory<LoadData<object>>>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlerFactories = new List<IFactory<IProductItemHandler>>();

        var categoryPagingList = new List<ICategoryPaging<TestCategoryProducts>>
        {
            new TestCategoryPaging(),
        };

        var categorySerializerList = new List<IItemSerializer<TestCategoryProducts>>
        {
            new TestItemSerializer(),
        };

        var serviceFactory = new TestProductServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger<TestProductService<TestCategoryProducts, TestProductItem>>>().Object,
            loaderServiceFactories,
            productItemHandlerFactories,
            categoryPagingList,
            categorySerializerList,
            [],
            [],
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = CreateImportSettingsMock();
        var importSettingsProviderMock = CreateImportSettingsProviderMock(importSettingsMock);

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProviderMock.Object, importSettingsMock.Object));
        Assert.Contains("ProductItemHandler", exception.Message);
    }

    [Fact]
    public void TestCreateProductServiceThrowsInvalidDataExceptionWhenCategoryPagingNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory<LoadData<object>>>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlerFactories = new List<IFactory<IProductItemHandler>>
        {
            new TestProductItemHandlerFactory(),
        };

        var categoryPagingList = new List<ICategoryPaging<TestCategoryProducts>>();

        var categorySerializerList = new List<IItemSerializer<TestCategoryProducts>>
        {
            new TestItemSerializer(),
        };

        var serviceFactory = new TestProductServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger<TestProductService<TestCategoryProducts, TestProductItem>>>().Object,
            loaderServiceFactories,
            productItemHandlerFactories,
            categoryPagingList,
            categorySerializerList,
            [],
            [],
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = CreateImportSettingsMock();
        var importSettingsProviderMock = CreateImportSettingsProviderMock(importSettingsMock);

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProviderMock.Object, importSettingsMock.Object));
        Assert.Contains("CategoryPaging", exception.Message);
    }

    [Fact]
    public void TestCreateProductServiceThrowsInvalidDataExceptionWhenCategorySerializerNotFound()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory<LoadData<object>>>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlerFactories = new List<IFactory<IProductItemHandler>>
        {
            new TestProductItemHandlerFactory(),
        };

        var categoryPagingList = new List<ICategoryPaging<TestCategoryProducts>>
        {
            new TestCategoryPaging(),
        };

        var categorySerializerList = new List<IItemSerializer<TestCategoryProducts>>();

        var serviceFactory = new TestProductServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger<TestProductService<TestCategoryProducts, TestProductItem>>>().Object,
            loaderServiceFactories,
            productItemHandlerFactories,
            categoryPagingList,
            categorySerializerList,
            [],
            [],
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = CreateImportSettingsMock();
        var importSettingsProviderMock = CreateImportSettingsProviderMock(importSettingsMock);

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProviderMock.Object, importSettingsMock.Object));
        Assert.Contains("CategorySerializer", exception.Message);
    }

    [Fact]
    public void TestCreateProductServiceThrowsInvalidDataExceptionWhenItemPathFormatterFactoryListIsEmpty()
    {
        var loaderServiceFactories = new List<ILoaderServiceFactory<LoadData<object>>>
        {
            new TestProductLoaderServiceFactory(),
        };

        var productItemHandlerFactories = new List<IFactory<IProductItemHandler>>
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

        var serviceFactory = new TestProductServiceFactory<TestCategoryProducts, TestProductItem, TestProductCatalogCategory>(
            new Mock<ILogger<TestProductService<TestCategoryProducts, TestProductItem>>>().Object,
            loaderServiceFactories,
            productItemHandlerFactories,
            categoryPagingList,
            categorySerializerList,
            [],
            [],
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = CreateImportSettingsMock();
        var importSettingsProviderMock = CreateImportSettingsProviderMock(importSettingsMock);

        var exception = Assert.Throws<InvalidDataException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProviderMock.Object, importSettingsMock.Object));
        Assert.Contains("ItemPathFormatter", exception.Message);
    }

    private static Mock<IImportSettings> CreateImportSettingsMock()
    {
        var importSettingsMock = new Mock<IImportSettings>();
        return importSettingsMock;
    }

    private static Mock<IImportSettingsProvider> CreateImportSettingsProviderMock(Mock<IImportSettings> importSettingsMock)
    {
        var importSettingsProviderMock = new Mock<IImportSettingsProvider>();

        var productServiceOptions = new ProductServiceOptions<object>
        {
            ProductPathFormatType = PathFormatType.ItemId,
            CategoryPathFormatType = PathFormatType.Path
        };
        importSettingsProviderMock.Setup(x => x.TryGetService<ProductServiceOptions<object>>(importSettingsMock.Object, "ProductServiceOptions", out productServiceOptions))
            .Returns(true);

        IServiceSettings loaderServiceSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestProductLoaderService"
        };
        importSettingsProviderMock.Setup(x => x.TryGetServiceSettings(importSettingsMock.Object, "Loader", out loaderServiceSettings))
            .Returns(true);

        IServiceSettings productItemHandlerSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestProductItemHandler"
        };
        importSettingsProviderMock.Setup(x => x.TryGetServiceSettings(importSettingsMock.Object, "ProductItemHandler", out productItemHandlerSettings))
            .Returns(true);

        IServiceSettings categoryPagingSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestCategoryPaging"
        };
        importSettingsProviderMock.Setup(x => x.TryGetServiceSettings(importSettingsMock.Object, "CategoryPaging", out categoryPagingSettings))
            .Returns(true);

        IServiceSettings categorySerializerSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestItemSerializer"
        };
        importSettingsProviderMock.Setup(x => x.TryGetServiceSettings(importSettingsMock.Object, "CategorySerializer", out categorySerializerSettings))
            .Returns(true);

        TestProductCatalogCategory[]? capturedCategories = new TestProductCatalogCategory[1];
        importSettingsProviderMock.Setup(x => x.TryGet<TestProductCatalogCategory[]>(importSettingsMock.Object, "CatalogCategories", out capturedCategories))
            .Returns(true);

        string productPathFormat = "products/{0}";
        importSettingsProviderMock.Setup(x => x.TryGetString(importSettingsMock.Object, "ProductPathFormat", out productPathFormat))
            .Returns(true);

        string categoryPathFormat = "categories/{0}";
        importSettingsProviderMock.Setup(x => x.TryGetString(importSettingsMock.Object, "CategoryPathFormat", out categoryPathFormat))
            .Returns(true);

        IServiceSettings itemPathFormatterSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestItemPathFormatter"
        };
        importSettingsProviderMock.Setup(x => x.TryGetServiceSettings(importSettingsMock.Object, "ItemPathFormatter", out itemPathFormatterSettings))
            .Returns(true);

        return importSettingsProviderMock;
    }
}
