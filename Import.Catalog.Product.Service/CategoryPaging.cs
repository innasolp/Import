using Import.Catalog.Product.Interfaces;

namespace Import.Catalog.Product.Service;

public abstract class CategoryPaging<TCategory> : ICategoryPaging<TCategory>
    where TCategory : class, ICategoryProducts
{
    protected virtual object[] GetCategoryPathArguments(IProductCatalogCategory productCatalogCategory, PathFormatType pathFormatType)
    {
        return pathFormatType switch
        {
            PathFormatType.ItemId => [productCatalogCategory.ItemId],
            PathFormatType.Path => [PreparePath(productCatalogCategory.Path)],
            PathFormatType.CategoryWithItemId => [PreparePath(productCatalogCategory.Category), productCatalogCategory.ItemId],
            _ => [productCatalogCategory.Path],
        };
    }

    protected abstract string PreparePath(string path);

    public virtual string GetCategoryPagePath(IProductCatalogCategory productCatalogCategory,
        string pathFormat,
        PathFormatType pathFormatType,
        int page,
        TCategory? category = null)
    {
        List<object> args = [.. GetCategoryPathArguments(productCatalogCategory, pathFormatType), page];

        return string.Format(pathFormat, args: [.. args]);
    }

    public virtual string GetNextCategoryPagePath(IProductCatalogCategory productCatalogCategory,
        string pathFormat,
        PathFormatType pathFormatType,
        int page,
        TCategory? category = null)
    {
        return GetCategoryPagePath(productCatalogCategory, pathFormat, pathFormatType, page + 1, category);
    }
}