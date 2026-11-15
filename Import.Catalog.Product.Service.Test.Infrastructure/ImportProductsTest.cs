using Import.Catalog.Product.Interfaces;
using Import.Service.Test.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using System.Resources;
using Xunit.Abstractions;

namespace Import.Catalog.Product.Service.Test.Infrastructure;

public abstract class ImportProductsTest<TService> : ImportServiceTest<TService, ILogger>
    where TService : ProductService<TestCategory, TestProductItem>
{
    protected ResourceManager ImportProductsResourceManager { get; }

    protected Mock<IProductCatalogModel> ProductCatalogModelMock { get; } = new();

    protected Mock<IProductItemHandler> ProductItemHandlerMock { get; } = new Mock<IProductItemHandler>();

    protected ImportProductsTest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        ImportProductsResourceManager = new ResourceManager("Import.Catalog.Product.Service.ImportProductLogMessages",
                               typeof(ProductService<TestCategory, TestProductItem>).Assembly);

        ProductCatalogModelMock.Setup(s => s.Categories).Returns([]);
    }
}