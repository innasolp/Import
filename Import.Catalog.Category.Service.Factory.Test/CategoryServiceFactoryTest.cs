using System.Text.Json;
using System.Text.Json.Nodes;
using Import.Catalog.Category.Factory.Interfaces;
using Import.Factory.Interfaces;
using Import.Settings.Interfaces;
using Import.Settings.Json;
using Import.TreeItem;
using Import.TreeItem.Html;
using Import.TreeItem.Json;
using Microsoft.Extensions.Logging;
using Moq;

namespace Import.Catalog.Category.Service.Factory.Test;

public class CategoryServiceFactoryTest
{
    [Fact]
    public void TestCreateCategoryServiceWithMultipleTreeLoadStagesSuccessWhenSettingsAreValid()
    {
        var treeItemLoadFactories = new List<ITreeItemLoaderFactory<TestCategory>>
        {
            new TreeItemJsonFromHtmlLoaderFactory<TestCategory>(),
            new JsonTreeItemLoaderFactory<TestCategory>(),
        };

        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestLoaderService1Factory(),
            new TestLoaderService2Factory(),
        };

        var categoryItemHandlerFactories = new List<ICategoryItemHandlerFactory<TestCategory>>
        {
            new TestCategoryItemHandler1Factory<TestCategory>(),
            new TestCategoryItemHandler2Factory<TestCategory>(),
        };

        var serviceFactory = new TestCategoryServiceFactory<TestCategory>(
            new Mock<ILogger<TestCategoryService<TestCategory>>>().Object,
            loaderServiceFactories,
            treeItemLoadFactories,
            categoryItemHandlerFactories,
            new Mock<IImportServiceLogFactory>().Object);

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var (importSettings, importSettingsProvider) = CreateImportSettingsMock();

        var service = serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProvider, importSettings);

        Assert.NotNull(service);
    }

    [Fact]
    public void TestCreateCategoryServiceThrowsInvalidOperationExceptionWhenLoaderFactoryCreateMethodThrowsIt()
    {
        var treeItemLoadFactories = new List<ITreeItemLoaderFactory<TestCategory>>
        {
            new TreeItemJsonFromHtmlLoaderFactory<TestCategory>(),
            new JsonTreeItemLoaderFactory<TestCategory>(),
        };

        var loaderServiceMock = new Mock<ILoaderServiceFactory>();
        var exceptionMessage = "TestLoaderService2Factory invalid";
        loaderServiceMock.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<IImportSettings>()))
            .Throws(new InvalidOperationException(exceptionMessage));
        loaderServiceMock.Setup(x => x.GetServiceType()).Returns(typeof(TestLoaderService2));

        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestLoaderService1Factory(),
            loaderServiceMock.Object,
        };

        var categoryItemHandlerFactories = new List<ICategoryItemHandlerFactory<TestCategory>>
        {
            new TestCategoryItemHandler1Factory<TestCategory>(),
            new TestCategoryItemHandler2Factory<TestCategory>(),
        };

        var serviceFactory = new TestCategoryServiceFactory<TestCategory>(
            new Mock<ILogger<TestCategoryService<TestCategory>>>().Object,
            loaderServiceFactories,
            treeItemLoadFactories,
            categoryItemHandlerFactories,
            new Mock<IImportServiceLogFactory>().Object);

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var (importSettings, importSettingsProvider) = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidOperationException>(()
            => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProvider, importSettings));

        Assert.Equal(exceptionMessage, exception.Message);
    }

    [Fact]
    public void TestCreateCategoryServiceThrowsExceptionWhenCategorySourceUrlNotFound()
    {
        var treeItemLoadFactories = new List<ITreeItemLoaderFactory<TestCategory>>
        {
            new TreeItemJsonFromHtmlLoaderFactory<TestCategory>(),
            new JsonTreeItemLoaderFactory<TestCategory>(),
        };

        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestLoaderService2Factory(),
        };

        var categoryItemHandlerFactories = new List<ICategoryItemHandlerFactory<TestCategory>>
        {
            new TestCategoryItemHandler2Factory<TestCategory>(),
        };

        var serviceFactory = new TestCategoryServiceFactory<TestCategory>(
            new Mock<ILogger<TestCategoryService<TestCategory>>>().Object,
            loaderServiceFactories,
            treeItemLoadFactories,
            categoryItemHandlerFactories,
            new Mock<IImportServiceLogFactory>().Object);

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = new Mock<IImportSettings>();
        var importSettingsProviderMock = new Mock<IImportSettingsProvider>();

        var exception = Assert.Throws<Exception>(()
            => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProviderMock.Object, importSettingsMock.Object));

        Assert.Contains("categorySourceUrl", exception.Message);
    }

    [Fact]
    public void TestCreateCategoryServiceThrowsExceptionWhenLoadStagesNotFound()
    {
        var treeItemLoadFactories = new List<ITreeItemLoaderFactory<TestCategory>>
        {
            new TreeItemJsonFromHtmlLoaderFactory<TestCategory>(),
            new JsonTreeItemLoaderFactory<TestCategory>(),
        };

        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestLoaderService2Factory(),
        };

        var categoryItemHandlerFactories = new List<ICategoryItemHandlerFactory<TestCategory>>
        {
            new TestCategoryItemHandler2Factory<TestCategory>(),
        };

        var serviceFactory = new TestCategoryServiceFactory<TestCategory>(
            new Mock<ILogger<TestCategoryService<TestCategory>>>().Object,
            loaderServiceFactories,
            treeItemLoadFactories,
            categoryItemHandlerFactories,
            new Mock<IImportServiceLogFactory>().Object);

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = new Mock<IImportSettings>();
        var importSettingsProviderMock = new Mock<IImportSettingsProvider>();
        string categorySourceUrl = "http://TestSource";
        importSettingsProviderMock.Setup(x => x.TryGetString(importSettingsMock.Object, It.Is<string>(s => s == "CategorySourceUrl"), out categorySourceUrl))
            .Returns(true);
        importSettingsProviderMock.Setup(x => x.TryGetService<LoadStageSettings[]>(importSettingsMock.Object, It.Is<string>(s => s == "LoadStages"), out It.Ref<LoadStageSettings[]>.IsAny))
            .Returns(false);

        var exception = Assert.Throws<Exception>(()
            => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProviderMock.Object, importSettingsMock.Object));

        Assert.Contains("loadStagesSettings", exception.Message);
    }

    [Fact]
    public void TestCreateCategoryServiceThrowsInvalidDataExceptionWhenLoaderFactoryNotFound()
    {
        var treeItemLoadFactories = new List<ITreeItemLoaderFactory<TestCategory>>
        {
            new TreeItemJsonFromHtmlLoaderFactory<TestCategory>(),
            new JsonTreeItemLoaderFactory<TestCategory>(),
        };

        var loaderServiceFactories = new List<ILoaderServiceFactory>();

        var categoryItemHandlerFactories = new List<ICategoryItemHandlerFactory<TestCategory>>
        {
            new TestCategoryItemHandler2Factory<TestCategory>(),
        };

        var serviceFactory = new TestCategoryServiceFactory<TestCategory>(
            new Mock<ILogger<TestCategoryService<TestCategory>>>().Object,
            loaderServiceFactories,
            treeItemLoadFactories,
            categoryItemHandlerFactories,
            new Mock<IImportServiceLogFactory>().Object);

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var (importSettings, importSettingsProvider) = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidDataException>(()
            => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProvider, importSettings));

        Assert.Contains("Loader", exception.Message);
    }

    [Fact]
    public void TestCreateCategoryServiceThrowsInvalidDataExceptionWhenCategoryItemHandlerNotFound()
    {
        var treeItemLoadFactories = new List<ITreeItemLoaderFactory<TestCategory>>
        {
            new TreeItemJsonFromHtmlLoaderFactory<TestCategory>(),
            new JsonTreeItemLoaderFactory<TestCategory>(),
        };

        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestLoaderService2Factory(),
        };

        var categoryItemHandlerFactories = new List<ICategoryItemHandlerFactory<TestCategory>>();

        var serviceFactory = new TestCategoryServiceFactory<TestCategory>(
            new Mock<ILogger<TestCategoryService<TestCategory>>>().Object,
            loaderServiceFactories,
            treeItemLoadFactories,
            categoryItemHandlerFactories,
            new Mock<IImportServiceLogFactory>().Object);

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var (importSettings, importSettingsProvider) = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidDataException>(()
            => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProvider, importSettings));

        Assert.Contains("CategoryItemHandler", exception.Message);
    }

    [Fact]
    public void TestCreateCategoryServiceThrowsInvalidDataExceptionWhenTreeItemLoaderFactoryNotFound()
    {
        var treeItemLoadFactories = new List<ITreeItemLoaderFactory<TestCategory>>();

        var loaderServiceFactories = new List<ILoaderServiceFactory>
        {
            new TestLoaderService2Factory(),
        };

        var categoryItemHandlerFactories = new List<ICategoryItemHandlerFactory<TestCategory>>
        {
            new TestCategoryItemHandler2Factory<TestCategory>(),
        };

        var serviceFactory = new TestCategoryServiceFactory<TestCategory>(
            new Mock<ILogger<TestCategoryService<TestCategory>>>().Object,
            loaderServiceFactories,
            treeItemLoadFactories,
            categoryItemHandlerFactories,
            new Mock<IImportServiceLogFactory>().Object);

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var (importSettings, importSettingsProvider) = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidDataException>(()
            => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsProvider, importSettings));

        Assert.Contains("TreeItemJsonFromHtmlLoader", exception.Message);
    }

    private static (IImportSettings, IImportSettingsProvider) CreateImportSettingsMock()
    {
        var importSettingsMock = new Mock<IImportSettings>();
        var importSettingsProviderMock = new Mock<IImportSettingsProvider>();
        string categorySourceUrl = "http://TestSource";
        importSettingsProviderMock.Setup(x => x.TryGetString(importSettingsMock.Object, It.Is<string>(s => s == "CategorySourceUrl"), out categorySourceUrl))
            .Returns(true);

        var loadStageSettings = new LoadStageSettings[]
        {
            new()
            {
                TypeName = "TreeItemJsonFromHtmlLoader",
                OptionsJson = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(
                    new HtmlTreeItemLoadOptions()
                    {
                        FirstNodePath = ["data", "columns", "categories"],
                        HtmlSearchFactoryOptions = new Html.Factory.HtmlSearchFactoryOptions()
                        {
                            SearchMatchType = Html.Factory.SearchMatchType.Like,
                            SearchElementType = Html.Factory.SearchElementType.Attribute
                        }
                    })),
            },
            new()
            {
                TypeName = "JsonTreeItemLoader",
                OptionsJson = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(
                    new HtmlTreeItemLoadOptions()
                    {
                        FirstNodePath = ["data", "columns", "categories"]
                    })),
            },
        };

        importSettingsProviderMock.Setup(x => x.TryGetService<LoadStageSettings[]>(importSettingsMock.Object, It.Is<string>(s => s == "LoadStages"), out loadStageSettings))
            .Returns(true);

        IServiceSettings loaderServiceSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestLoaderService2"
        };
        importSettingsProviderMock.Setup(x => x.TryGetServiceSettings(importSettingsMock.Object, It.Is<string>(s => s == "Loader"), out loaderServiceSettings))
            .Returns(true);

        IServiceSettings categoryItemHandlerSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestCategoryItemHandler2"
        };
        importSettingsProviderMock.Setup(x => x.TryGetServiceSettings(importSettingsMock.Object, It.Is<string>(s => s == "CategoryItemHandler"), out categoryItemHandlerSettings))
            .Returns(true);

        return (importSettingsMock.Object, importSettingsProviderMock.Object);
    }
}
