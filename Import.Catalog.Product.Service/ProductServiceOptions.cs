using Import.Catalog.Product.Interfaces;

namespace Import.Catalog.Product.Service;

public class ProductServiceOptions
{
    public int? PageProductCount { get; set; }

    public object? ProductLoadData { get; set; }

    public object? CategoryLoadData { get; set; }

    public PathFormatType ProductPathFormatType { get; set; }

    public PathFormatType CategoryPathFormatType { get; set; }
}