using Import.Catalog.Product.Interfaces;
using Import.Catalog.Product.Service.Test.Infrastructure;
using Import.KeyHash;
using Import.Service.Test;
using Import.Service.Test.Infrastructure;
using Import.ServiceState;
using Microsoft.Extensions.Logging;
using Moq;

using Xunit.Abstractions;

namespace Import.Catalog.Product.Service.Stateful.Test;

public class ImportCatalogProductServiceTest : ImportServiceExecutionTest<TestImportProductStatefulService<TestCategory, TestProductItem>, ILogger>
{
    private readonly Mock<IKeyHasher> _keyHasherMock = new();

    private readonly Mock<IServiceStateRepository> _serviceStarepositoryMock = new();

    public ImportCatalogProductServiceTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        ProductCatalogModelMock.Setup(s => s.Categories).Returns([]);
    }

    protected override TestImportProductStatefulService<TestCategory, TestProductItem> CreateService(string name)
    {
        return new TestImportProductStatefulService<TestCategory, TestProductItem>(
             LoggerMock.Object,
             name,
             ProductCatalogModelMock.Object,
             LoaderMock.Object,
             ProductItemHandlerMock.Object,
             _keyHasherMock.Object,
             _serviceStarepositoryMock.Object);
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
        LoaderMock.SetupStartSuccess();
        LoaderMock.Setup(s => s.Name).Returns($"{Guid.NewGuid()}");

        var categoryMock = TestHelper.CreateProductCatalogCategoryMock();
        ProductCatalogModelMock.Object.Categories.Add(categoryMock.Object);

        ProductCatalogModelMock.Setup(s => s.CategoryUrlFormat).Returns($"{Guid.NewGuid()}_{{0}}_{{1}}");

        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);

        LoaderMock.Setup(s => s.Load(It.IsAny<string>(), It.IsAny<object?>(), It.IsAny<CancellationToken>())).
            Returns(async (string path, object? data, CancellationToken cancellationToken) =>
            {
                await Task.Delay(100, cancellationToken);
                return default;
            });

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

        await ShouldLogServiceFailedErrorWhenUnhandledExceptionThrownAsync(1000);
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