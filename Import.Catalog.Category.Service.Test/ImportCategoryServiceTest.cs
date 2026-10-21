using Import.Catalog.Category.Interfaces;
using Import.Service.Test;
using Import.Service.Test.Infrastructure;
using Import.TreeItem;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Import.Catalog.Category.Service.Test;

public class ImportCategoryServiceTest : ImportServiceExecutionTest<CategoryTimerServiceTest, ILogger<CategoryTimerServiceTest>>
{
    private readonly Mock<ICategoryShopModel> _categoryShopModelMock = new();

    private readonly int _secondsInterval = 1;

    private readonly Mock<ITreeItemLoader<TestCategory>>[] _categoryLoadersMock = [new Mock<ITreeItemLoader<TestCategory>>()];

    private readonly Mock<ITreeItemLoader<TestCategory>> _categoryLoaderMock = new();

    private readonly Mock<ICategoryItemHandler<TestCategory>> _categoryItemHandlerMock = new();


    public ImportCategoryServiceTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _categoryShopModelMock.Setup(s => s.CategorySourceUrl).Returns(Guid.NewGuid().ToString());
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

    [Fact]
    public async Task ShouldLogImportWasStoppedWithErrorWhenLoaderNotExecuted()
    {
        LoaderMock.Reset();
        await ShouldLogImportWasStoppedWithErrorWhenLoaderNotExecutedAsync(3000);
    }

    [Fact]
    public async Task ShouldLogServiceStartedWhenLoaderExecutesSuccessfully()
    {
        LoaderMock.Reset();

        var requestData = new { id = 2 };
        var category = Helper.CreateCategoryWithChildren();
        LoaderMock.SetupLoadItem(_categoryShopModelMock.Object.CategorySourceUrl, requestData, category);

        var categoryStream = await TestExtensions.LoadItemAsync(category, CancellationToken.None);

        LoaderMock.SetupGetRequestData(requestData);
        LoaderMock.Setup(w =>
           w.Load(_categoryShopModelMock.Object.CategorySourceUrl,
           It.Is<object?>(data => data == requestData),
           It.IsAny<CancellationToken>()))
           .ReturnsAsync(categoryStream);

        _categoryLoaderMock.Setup(s => s.LoadAsync(null, It.Is<Stream>(s => s == categoryStream), It.IsAny<CancellationToken>())).
            ReturnsAsync([category]);

        await ShouldLogServiceStartedWhenLoaderExecutesSuccessfullyAsync(1000);
    }

    [Fact]
    public async Task ShouldLogImportStoppedWhenCancellationRequested()
    {
        LoaderMock.Reset();

        var requestData = new { id = 2 };
        var category = Helper.CreateCategoryWithChildren();
        LoaderMock.SetupLoadItem(_categoryShopModelMock.Object.CategorySourceUrl, requestData, category);

        var categoryStream = await TestExtensions.LoadItemAsync(category, CancellationToken.None);

        LoaderMock.SetupGetRequestData(requestData);

        _categoryLoaderMock.Setup(s => s.LoadAsync(null, It.Is<Stream>(s => s == categoryStream), It.IsAny<CancellationToken>())).
            ReturnsAsync([category]);

        await ShouldLogImportStoppedWhenCancellationRequestedAsync(500);
    }

    [Fact]
    public async Task ShouldLogResettingErrorIfLoaderServiceNeedsReseting()
    {
        LoaderMock.Reset();

        await ShouldLogResettingErrorIfLoaderServiceNeedsResettingAsync(15000);
    }

    [Fact]
    public async Task ShouldLogServiceFailedErrorWhenUnhandledExceptionThrown()
    {
        LoaderMock.Reset();

        await ShouldLogServiceFailedErrorWhenUnhandledExceptionThrownAsync(1000);
    }

    [Fact]
    public async Task ShouldLogWarningAboutRetryAndLogSuccessAfterRetry()
    {
        LoaderMock.Reset();

        await ShouldLogWarningAboutRetryAndLogSuccessAfterRetryAsync(2000, TimeSpan.FromMilliseconds(1000));
    }
}