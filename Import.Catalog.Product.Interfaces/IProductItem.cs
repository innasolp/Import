namespace Import.Catalog.Product.Interfaces;

public interface IProductItem
{
    string Name { get; set; }

    string ItemId { get; set; }

    string Brand { get; set; }

    string Currency { get; set; }

    double Price { get; set; }

    string Path { get; set; }

    string AbsolutePath { get; set; }

    int CategoryId { get; set; }
}