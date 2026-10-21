using Import.Catalog.Category.Interfaces;
using Import.Interfaces.Exceptions;
using Import.Service.Test;
using Import.Service.Test.Infrastructure;
using Import.TreeItem;
using Microsoft.Extensions.Logging;
using Moq;
using System.Resources;
using Xunit.Abstractions;

namespace Import.Catalog.Category.Service.Test;

public class ImportCategoryJsonLoadingTest : ImportServiceTest<CategoryTimerServiceTest, ILogger<CategoryTimerServiceTest>>
{
    private readonly Mock<ICategoryShopModel> _categoryShopModelMock = new();

    private readonly int _secondsInterval = 30;

    private readonly Mock<ICategoryItemHandler<TestCategory>> _categoryItemHandlerMock = new();

    private readonly Mock<ITreeItemLoader<TestCategory>> _categoryLoaderMock = new();

    private readonly Mock<ITreeItemLoader<TestCategory>>[] _categoryLoadersMock;

    protected ResourceManager ImportCategoriesResourceManager { get; }

    public ImportCategoryJsonLoadingTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _categoryLoadersMock = [_categoryLoaderMock];

        ImportCategoriesResourceManager = new ResourceManager("Import.Catalog.Category.Service.ImportCategoryLogMessages",
                               typeof(CategoryService<>).Assembly);

        LoaderMock.SetupLoadCookies();
        _categoryShopModelMock.Setup(s => s.CategorySourceUrl).Returns(Guid.NewGuid().ToString());
        _categoryShopModelMock.Setup(s => s.Name).Returns(Guid.NewGuid().ToString());
        _categoryShopModelMock.Setup(s => s.SourceName).Returns(Guid.NewGuid().ToString());
        _categoryShopModelMock.Setup(s => s.SourceUrl).Returns(Guid.NewGuid().ToString());
    }

    protected override CategoryTimerServiceTest CreateService(string name)
    {
        return new CategoryTimerServiceTest(LoggerMock.Object,
            name,
            LoaderMock.Object,
            _categoryShopModelMock.Object,
            _categoryLoadersMock.Select(m => m.Object),
            _secondsInterval,
            _categoryItemHandlerMock.Object
            );
    }

    private async Task ExecuteServiceAsync(string name, int executionDuration)
    {
        var service = CreateService(name);

        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(executionDuration);

        await service.Start(tokenSource.Token);
    }

    [Fact]
    public async Task LogInfoSuccessWhenCategoriesLoaded()
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();
        LoaderMock.Setup(s => s.Name).Returns($"{Guid.NewGuid()}");

        var category = Helper.CreateCategoryWithChildren();

        var categoryStream = await TestExtensions.LoadItemAsync(category, CancellationToken.None);

        var requestData = new { id = 2 };
        var loadAutoResetEvent = new AutoResetEvent(true);
        LoaderMock.SetupGetRequestData(requestData);
        LoaderMock.Setup(w =>
           w.Load(_categoryShopModelMock.Object.CategorySourceUrl,
           It.Is<object?>(data => data == requestData),
           It.IsAny<CancellationToken>()))
           .Returns(async (string url, object? data, CancellationToken cancellationToken) =>
           {
               var stream = categoryStream;
               loadAutoResetEvent.Set();
               return stream;
           });

        var name = Guid.NewGuid().ToString();

        _categoryLoaderMock.Setup(s => s.LoadAsync(null, It.Is<Stream>(s => s == categoryStream), It.IsAny<CancellationToken>())).
            ReturnsAsync([category]);

        await ExecuteServiceAsync(name, 1000);

        LoggerMock.VerifyInfo(ImportCategoriesResourceManager.GetString("CategoryNameIdForShopWasLoaded"),
            category.Name, category.Id, _categoryShopModelMock.Object.SourceName);
    }

    [Fact]
    public async Task LogErrorWhenJsonLoadFromCategorySourceUrlFailed()
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();
        LoaderMock.Setup(s => s.Name).Returns($"{Guid.NewGuid()}");

        var exception = new NoActionLoaderServiceException("Json loading failed");
        var requestData = new { id = 10 };
        LoaderMock.SetupGetRequestData(requestData);
        LoaderMock.Setup(w =>
            w.Load(It.Is<string>(url => url == _categoryShopModelMock.Object.CategorySourceUrl),
            It.Is<object?>(d => d == requestData),
            It.IsAny<CancellationToken>()))
            .Throws(exception);

        var name = Guid.NewGuid().ToString();

        await ExecuteServiceAsync(name, 1000);

        LoggerMock.VerifyInfo(ImportCategoriesResourceManager.GetString("CategoriesWereNotLoaded"),
            _categoryShopModelMock.Object.SourceName, _categoryShopModelMock.Object.SourceUrl);
    }
}