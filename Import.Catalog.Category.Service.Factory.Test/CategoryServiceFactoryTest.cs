using System.Text.Json;
using System.Text.Json.Nodes;
using Import.Catalog.Category.Interfaces;
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

        var categoryItemHandlers = new List<ICategoryItemHandler<TestCategory>>
            {
                new TestCategoryItemHandler1<TestCategory>(),
                new TestCategoryItemHandler2<TestCategory>()
            };

        var serviceFactory = new TestCategoryServiceFactory<TestCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            treeItemLoadFactories,
            categoryItemHandlers,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = CreateImportSettingsMock();

        var service = serviceFactory.Create("TestService", importSourceMock.Object, importSettingsMock);
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

        var categoryItemHandlers = new List<ICategoryItemHandler<TestCategory>>
        {
            new TestCategoryItemHandler1<TestCategory>(),
            new TestCategoryItemHandler2<TestCategory>()
        };

        var serviceFactory = new TestCategoryServiceFactory<TestCategory>(
            new Mock<ILogger>().Object,
            loaderServiceFactories,
            treeItemLoadFactories,
            categoryItemHandlers,
            new Mock<IImportServiceLogFactory>().Object
        );

        var importSourceMock = new Mock<IImportSource>();
        importSourceMock.Setup(x => x.Name).Returns("TestSource");
        importSourceMock.Setup(x => x.Url).Returns("http://TestSource");

        var importSettingsMock = CreateImportSettingsMock();

        var exception = Assert.Throws<InvalidOperationException>(() => serviceFactory.Create("TestService", importSourceMock.Object, importSettingsMock));
        Assert.Equal(exceptionMessage, exception.Message);
    }

    private static IImportSettings CreateImportSettingsMock()
    {
        var importSettingsMock = new Mock<IImportSettings>();
        string categorySourceUrl = "http://TestSource";
        importSettingsMock.Setup(x => x.TryGetString(It.Is<string>(s => s == "CategorySourceUrl"), out categorySourceUrl))
        .Returns(true);

        var loadStageSettings = new LoadStageSettings[]
        {
            new(){ TypeName = "TreeItemJsonFromHtmlLoader",
             OptionsJson = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(
                new HtmlTreeItemLoadOptions(){
                    FirstNodePath = ["data","columns","categories"],
                    HtmlSearchFactoryOptions = new Html.Factory.HtmlSearchFactoryOptions(){
                        SearchMatchType = Html.Factory.SearchMatchType.Like,
                        SearchElementType = Html.Factory.SearchElementType.Attribute
                    }
                    })),  },
            new(){ TypeName = "JsonTreeItemLoader",
             OptionsJson = JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(
                new HtmlTreeItemLoadOptions(){
                    FirstNodePath = ["data","columns","categories"]
                    })),  }
        };
        importSettingsMock.Setup(x => x.TryGetService<LoadStageSettings[]>(It.Is<string>(s => s == "LoadStages"), out loadStageSettings))
        .Returns(true);

        IServiceSettings loaderServiceSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestLoaderService2"
        };
        importSettingsMock.Setup(x => x.TryGetServiceSettings(It.Is<string>(s => s == "Loader"), out loaderServiceSettings))
        .Returns(true);

        IServiceSettings categoryItemHandlerSettings = new ServiceSettings
        {
            ImplementationTypeName = "TestCategoryItemHandler2"
        };
        importSettingsMock.Setup(x => x.TryGetServiceSettings(It.Is<string>(s => s == "CategoryItemHandler"), out categoryItemHandlerSettings))
        .Returns(true);

        return importSettingsMock.Object;
    }
}
