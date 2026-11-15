using Import.Catalog.Product.Interfaces;

namespace Import.Catalog.Product.Service;

public abstract class CategoryItemPaging<TCategory> : CategoryPaging<TCategory>
    where TCategory : class, ICategoryProducts, IPagingItem
{
    public override string GetCategoryPagePath(IProductCatalogCategory productCatalogCategory, string pathFormat, PathFormatType pathFormatType, int page, TCategory? category = null)
    {
        return category == null
           ? base.GetCategoryPagePath(productCatalogCategory, pathFormat, pathFormatType, page, category)
           : category.GetPageUrl(pathFormat, $"{productCatalogCategory.ItemId}", page)
           ?? base.GetCategoryPagePath(productCatalogCategory, pathFormat, pathFormatType, page, category);
    }

    public override string GetNextCategoryPagePath(IProductCatalogCategory productCatalogCategory, string pathFormat, PathFormatType pathFormatType, int page, TCategory? category = null)
    {
        return category?.GetNextPageUrl(pathFormat, $"{productCatalogCategory.ItemId}", page)
           ?? base.GetNextCategoryPagePath(productCatalogCategory, pathFormat, pathFormatType, page, category);
    }
}