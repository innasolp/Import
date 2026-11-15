namespace Import.Catalog.Product.Interfaces;

public interface IProductCatalogCategory
{
    string Category { get; set; }

    int ItemId { get; set; }

    public string Path { get; set; }
}