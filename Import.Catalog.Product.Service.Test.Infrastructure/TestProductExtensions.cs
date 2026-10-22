using Import.Catalog.Product.Interfaces;

namespace Import.Catalog.Product.Service.Test.Infrastructure;

public static class TestProductExtensions
{
    public static string GetCategoryPageUrl(this IProductCatalogModel productCatalogModel, IProductCatalogCategory category, int page)
    {
        return string.Format(productCatalogModel.CategoryUrlFormat, category.Path, page);
    }
}