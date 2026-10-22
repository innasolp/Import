using Import.Catalog.Product.Interfaces;

namespace Import.Catalog.Product.Service;

public interface ICategoryPaging<TCategory>
    where TCategory : class, ICategoryProducts
{
    string GetCategoryPagePath(IProductCatalogCategory productCatalogCategory, string pathFormat, PathFormatType pathFormatType, int page, TCategory? category = null);

    string GetNextCategoryPagePath(IProductCatalogCategory productCatalogCategory, string urlFormat, PathFormatType urlFormatType, int page, TCategory? category = null);
}