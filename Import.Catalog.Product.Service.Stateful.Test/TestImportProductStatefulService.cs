using Import.Catalog.Product.Interfaces;
using Import.Catalog.Product.Service.Test.Infrastructure;
using Import.Interfaces;
using Import.KeyHash;
using Import.ServiceState;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Stateful.Test;

public class TestImportProductStatefulService<TCategory, TProductItem>(
    ILogger logger,
    string name,
    IProductCatalogModel catalogModel,
    ILoaderService<LoadData<object>> loader,
    IProductItemHandler itemHandler,
    IItemPathFormatter<ICategoryProductItem> productItemPathFormatter,
    IKeyHasher keyHasher,
    IServiceStateRepository serviceStateRepository,
    int pageProductCount = 1000)
    : ProductStatefulService<object, TCategory, TProductItem>(
        logger,
        loader,
        catalogModel.Url,
        catalogModel.Categories,
        itemHandler,
        catalogModel.ProductUrl,
        catalogModel.CategoryUrlFormat,
        catalogModel.Name,
        new TestCategoryPaging<TCategory>(),
        new SimpleCategoryJsonSerializer<TCategory>(),
        productItemPathFormatter,
        new ProductServiceOptions<object> { PageProductCount = pageProductCount },
        keyHasher,
        serviceStateRepository)
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    private readonly IProductCatalogModel _productCatalogModel = catalogModel;

    public override string Name { get; } = name;

    public string GetTestApiUrl(ICategoryProductItem productItem)
    {
        return productItemPathFormatter.GetAbsolutePath(
            _productCatalogModel.ProductUrl,
            PathFormatType.Path,
            productItem);
    }

    protected override bool? IsEndOfCategory(TCategory category, int processProductCount)
    {
        return !(category.CategoryProductItems?.Length > 0);
    }
}
