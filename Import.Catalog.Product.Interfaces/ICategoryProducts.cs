namespace Import.Catalog.Product.Interfaces;

public interface ICategoryProducts
{
    ICategoryProductItem[] CategoryProductItems { get; }

    int? TotalCount { get; }
}