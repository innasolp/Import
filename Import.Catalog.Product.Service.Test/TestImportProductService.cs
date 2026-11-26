using Import.Catalog.Product.Interfaces;
using Import.Catalog.Product.Service.Test.Infrastructure;
using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Test;

public class TestImportProductService<TCategory, TProductItem>(
    ILogger logger,
    string name,
    IProductCatalogModel catalogModel,
    ILoaderService<LoadData<object>> loader,
    IProductItemHandler itemHandler,
    IItemPathFormatter<ICategoryProductItem> productItemPathFormatter,
    int pageProductCount = 1000)
    : ProductService<object, TCategory, TProductItem>(
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
        new ProductServiceOptions<object> { PageProductCount = pageProductCount })
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    private readonly IItemPathFormatter<ICategoryProductItem> _productItemPathFormatter = productItemPathFormatter;

    private readonly IProductCatalogModel _productCatalogModel = catalogModel;

    public override string Name { get; } = name;

    public string GetTestApiUrl(ICategoryProductItem productItem)
    {
        return _productItemPathFormatter.GetAbsolutePath(
            _productCatalogModel.ProductUrl,
            PathFormatType.Path,
            productItem);
    }

    protected override bool? IsEndOfCategory(TCategory category, int processProductCount)
    {
        return !(category.CategoryProductItems?.Length > 0);
    }
}
