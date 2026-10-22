using Import.Catalog.Product.Interfaces;

namespace Import.Catalog.Product.Service.Stateful;

internal class ProductCatalogCategory : IProductCatalogCategory
{
    public string Category { get; set; }

    public int ItemId { get; set; }

    public string Path { get; set; }
}
