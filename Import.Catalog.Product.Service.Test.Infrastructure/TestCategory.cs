using Import.Catalog.Product.Interfaces;

namespace Import.Catalog.Product.Service.Test.Infrastructure;

public class TestCategory : ICategoryProducts
{
    public TestCategoryProduct[] CategoryProductItems { get; set; }

    public int? TotalCount { get; set; }

    ICategoryProductItem[] ICategoryProducts.CategoryProductItems => CategoryProductItems;

    public int Page { get; set; }
}

public class TestCategoryProduct : ICategoryProductItem
{
    public string Id { get; set; }

    public string ItemPath { get; set; }

    public string Currency { get; set; }

    public double Price { get; set; }

    public string Name { get; set; }

    public int CategoryItemId { get; set; }
    public string Brand { get; set; }
}

public class TestProductItem : IProductItem
{
    public string ItemId { get; set; }

    public string Name { get; set; }

    public string[]? Components { get; set; }

    public string Brand { get; set; }

    public string Country { get; set; }

    public string Comment { get; set; }

    public string ProductType { get; set; }

    public string[] Purposes { get; set; }

    public string Articul { get; set; }

    public string Currency { get; set; }
    public double Price { get; set; }
    public string Path { get; set; }
    public string AbsolutePath { get; set; }
    public int CategoryId { get; set; }
}
