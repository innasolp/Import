using Import.Catalog.Product.Interfaces;

namespace Import.Catalog.Product.Service.Test.Infrastructure;

public interface IProductCatalogModel
{
    string ProductUrl { get; }
    string CategoryUrlFormat { get; }
    List<IProductCatalogCategory> Categories { get; }
    string Name { get; }
    string Url { get; }
}