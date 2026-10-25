using Import.Catalog.Product.Interfaces;

namespace Import.Catalog.Product.Service.Factory;

internal class ProductCatalogCategoryImpl : IProductCatalogCategory
{
    public string Category { get; set; } = string.Empty;

    public int ItemId { get; set; }

    public string Path { get; set; } = string.Empty;
}
