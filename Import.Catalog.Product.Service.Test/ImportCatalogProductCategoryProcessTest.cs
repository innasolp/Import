using Import.Catalog.Product.Interfaces;
using Import.Catalog.Product.Service.Test.Infrastructure;
using Import.Interfaces;
using Import.Interfaces.Exceptions;
using Import.Service.Test.Infrastructure;
using Moq;
using Xunit.Abstractions;

namespace Import.Catalog.Product.Service.Test;

public class ImportCatalogProductCategoryProcessTest : ImportProductsTest<TestImportProductService<TestCategory, TestProductItem>>
{
    protected override TestImportProductService<TestCategory, TestProductItem> CreateService(string name)
    {
        return new TestImportProductService<TestCategory, TestProductItem>(
            LoggerMock.Object,
            name,
             ProductCatalogModelMock.Object,
             LoaderMock.Object,
             ProductItemHandlerMock.Object,
             1000);
    }

    protected TestImportProductService<TestCategory, TestProductItem> CreateService(string name, int pageProductCount)
    {
        return new TestImportProductService<TestCategory, TestProductItem>(
            LoggerMock.Object,
            name,
             ProductCatalogModelMock.Object,
             LoaderMock.Object,
             ProductItemHandlerMock.Object,
             pageProductCount);
    }

    public ImportCatalogProductCategoryProcessTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        LoaderMock.SetupLoadCookies();
        ProductCatalogModelMock.Setup(s => s.ProductUrl).Returns("Product_{0}");
        ProductCatalogModelMock.Setup(s => s.CategoryUrlFormat).Returns("Category_{0}_page{1}");
    }

    private void SetupLoaderWithCategoryLoadException(Exception exception, out string categoryUrl)
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();

        var categoryMock = TestHelper.CreateProductCatalogCategoryMock();
        ProductCatalogModelMock.Object.Categories.Add(categoryMock.Object);

        ProductCatalogModelMock.Setup(s => s.CategoryUrlFormat).Returns($"{Guid.NewGuid()}_{{0}}");

        var url = ProductCatalogModelMock.Object.GetCategoryPageUrl(categoryMock.Object, 1);
        LoaderMock.Setup(w => w.Load(url, It.IsAny<object>(), It.IsAny<CancellationToken>())).Throws(exception);

        categoryUrl = url;
    }

    private TestImportProductService<TestCategory, TestProductItem> SetupServiceWithCategoryProcessException(string name, Func<string, Exception> getItemException, int pageCount, out TestCategory testCategory, out string url)
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();
        LoaderMock.Setup(s => s.Name).Returns($"{Guid.NewGuid()}");

        var categoryMock = TestHelper.CreateProductCatalogCategoryMock();
        ProductCatalogModelMock.Object.Categories.Add(categoryMock.Object);

        ProductCatalogModelMock.Setup(s => s.CategoryUrlFormat).Returns(Guid.NewGuid().ToString());

        var categoryUrl = ProductCatalogModelMock.Object.GetCategoryPageUrl(categoryMock.Object, 1);
        var productCount = new Random().Next(10, 20);
        var categoryProducts = TestHelper.CreateCategoryWithProducts(productCount, productCount, 1);

        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);

        LoaderMock.SetupLoadItem(categoryUrl, requestData, categoryProducts);

        var service = CreateService(name, pageCount);

        var productItems = categoryProducts.CategoryProductItems.ToDictionary(service.GetTestApiUrl, TestHelper.CreateProductItem);
        LoaderMock.SetupLoadItemsThrowsExceptions(productItems.Keys, getItemException, requestData);

        ProductItemHandlerMock.Setup(s => s.HandleItem(It.IsAny<IImportProduct>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(ResultStatus.Success));

        testCategory = categoryProducts;
        url = categoryUrl;

        return service;
    }

    private TestImportProductService<TestCategory, TestProductItem> SetupServiceWithCategoryLoadSuccessfull(string name, int pageCount, out TestCategory[] testCategoryPages, out string[] categoryPageUrls)
    {
        LoaderMock.Reset();
        LoaderMock.SetupStartSuccess();
        LoaderMock.Setup(s => s.Name).Returns($"{Guid.NewGuid()}");

        var categoryMock = TestHelper.CreateProductCatalogCategoryMock();
        ProductCatalogModelMock.Object.Categories.Add(categoryMock.Object);

        ProductCatalogModelMock.Setup(s => s.CategoryUrlFormat).Returns($"{Guid.NewGuid()}_{{0}}_{{1}}");

        var requestData = new object();
        LoaderMock.SetupGetRequestData(requestData);

        var service = CreateService(name, 10);

        categoryPageUrls = new string[pageCount];
        testCategoryPages = new TestCategory[pageCount];

        for (int i = 0; i < pageCount; i++)
        {
            var categoryPageUrl = ProductCatalogModelMock.Object.GetCategoryPageUrl(categoryMock.Object, i + 1);
            var productCount = 10;
            var categoryProducts = TestHelper.CreateCategoryWithProducts(productCount, productCount * pageCount, i + 1);
            LoaderMock.SetupLoadItem(categoryPageUrl, requestData, categoryProducts);

            var productItems = categoryProducts.CategoryProductItems.ToDictionary(service.GetTestApiUrl, TestHelper.CreateProductItem);
            LoaderMock.SetupLoadItemsSuccessfull(productItems, requestData);

            categoryPageUrls[i] = categoryPageUrl;
            testCategoryPages[i] = categoryProducts;
        }

        ProductItemHandlerMock.Setup(s => s.HandleItem(It.IsAny<IImportProduct>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(ResultStatus.Success));

        return service;
    }

    [Fact]
    public async Task ImportLogErrorWhenAllCategoryProductsNotProcessed()
    {
        var exceptionFormat = "test exception {0}";
        var service = SetupServiceWithCategoryProcessException(Guid.NewGuid().ToString(),
            item => new NoActionLoaderServiceException(string.Format(exceptionFormat, item)),
            10,
            out var categoryProducts,
            out var categoryUrl);

        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(500);

        await service.Start(tokenSource.Token);

        LoaderMock.Verify(l => l.Load(It.Is<string>(v => v == categoryUrl), It.IsAny<object>(), It.IsAny<CancellationToken>()));

        foreach (var item in categoryProducts.CategoryProductItems)
        {
            LoggerMock.VerifyInfo(ImportProductsResourceManager.GetString("ProductWasNotLoadedFromUrlWithError"), service.GetTestApiUrl(item));
        }

        LoggerMock.VerifyInfo(ImportProductsResourceManager.GetString("CategoryProductsWereNotLoadedError"), categoryUrl);
    }

    [Fact]
    public async Task ImportLogWhenCategoryCompleted()
    {
        var pageCount = 2;
        var service = SetupServiceWithCategoryLoadSuccessfull(Guid.NewGuid().ToString(), pageCount, out var categoryProducts, out var categoryUrls);

        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfter(1000);

        await service.Start(tokenSource.Token);

        foreach (var categoryUrl in categoryUrls)
            LoaderMock.Verify(l => l.Load(It.Is<string>(v => v.Contains(categoryUrl)), It.IsAny<object>(), It.IsAny<CancellationToken>()));

        foreach (var item in categoryProducts.SelectMany(c => c.CategoryProductItems))
        {
            LoggerMock.VerifyInfo(ImportProductsResourceManager.GetString("ProductHasBeenSuccessfullyLoadedFromUrl"), item.Name, service.GetTestApiUrl(item));
        }

        LoggerMock.VerifyInfo(ImportProductsResourceManager.GetString("CategoryCompletedInfo"),
            ProductCatalogModelMock.Object.Categories[0].Category, categoryProducts.SelectMany(c => c.CategoryProductItems).Count(), 0);
    }
}