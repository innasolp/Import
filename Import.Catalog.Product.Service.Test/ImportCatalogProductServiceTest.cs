using Import.Catalog.Product.Interfaces;
using Import.Catalog.Product.Service.Test.Infrastructure;
using Import.Service.Test.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Import.Catalog.Product.Service.Test;

public class ImportCatalogProductServiceTest : ImportServiceExecutionTest<TestImportProductService<TestCategory, TestProductItem>, ILogger>
{
    public ImportCatalogProductServiceTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        ProductCatalogModelMock.Setup(s => s.Categories).Returns([]);
    }

    protected override TestImportProductService<TestCategory, TestProductItem> CreateService(string name)
    {
        return new TestImportProductService<TestCategory, TestProductItem>(
             LoggerMock.Object,
             name,
             ProductCatalogModelMock.Object,
             LoaderMock.Object,
             ProductItemHandlerMock.Object);
    }

    protected Mock<IProductCatalogModel> ProductCatalogModelMock { get; } = new Mock<IProductCatalogModel>();

    protected Mock<IProductItemHandler> ProductItemHandlerMock { get; } = new Mock<IProductItemHandler>();

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
        await ShouldLogServiceStartedWhenLoaderExecutesSuccessfullyAsync(1000);
    }

    [Fact]
    public async Task ShouldLogImportStoppedWhenCancellationRequested()
    {
        LoaderMock.Reset();
        await ShouldLogImportStoppedWhenCancellationRequestedAsync(500);
    }

    [Fact]
    public async Task ShouldLogResettingErrorIfLoaderServiceNeedsReseting()
    {
        LoaderMock.Reset();

        ProductCatalogModelMock.Setup(s => s.CategoryUrlFormat).Returns("Category_{0}_page{1}");
        var categoryMock = TestHelper.CreateProductCatalogCategoryMock();
        ProductCatalogModelMock.Object.Categories.Add(categoryMock.Object);

        await ShouldLogResettingErrorIfLoaderServiceNeedsResettingAsync(15000);
    }

    [Fact]
    public async Task ShouldLogServiceFailedErrorWhenUnhandledExceptionThrown()
    {
        LoaderMock.Reset();

        ProductCatalogModelMock.Setup(s => s.CategoryUrlFormat).Returns("Category_{0}_page{1}");
        var categoryMock = TestHelper.CreateProductCatalogCategoryMock();
        ProductCatalogModelMock.Object.Categories.Add(categoryMock.Object);

        await ShouldLogServiceFailedErrorWhenUnhandledExceptionThrownAsync(2000);
    }

    [Fact]
    public async Task ShouldLogWarningAboutRetryAndLogSuccessAfterRetry()
    {
        LoaderMock.Reset();

        ProductCatalogModelMock.Setup(s => s.CategoryUrlFormat).Returns("Category_{0}_page{1}");
        var categoryMock = TestHelper.CreateProductCatalogCategoryMock();
        ProductCatalogModelMock.Object.Categories.Add(categoryMock.Object);

        await ShouldLogWarningAboutRetryAndLogSuccessAfterRetryAsync(1000, TimeSpan.FromMilliseconds(500));
    }
}