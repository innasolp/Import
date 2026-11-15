namespace Import.Catalog.Product.Interfaces;

public interface ICategoryProductItem
{
    string Id { get; }

    string ItemPath { get; }

    string Currency { get; }

    double Price { get; }

    string Name { get; }

    int CategoryItemId { get; set; }

    string? Brand { get; set; }
}