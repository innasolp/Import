using Import.Catalog.Product.Interfaces;

namespace Import.Catalog.Product.Service;

public class ProductServiceOptions<TLoadData>
{
    public int? PageProductCount { get; set; }

    public TLoadData? ProductLoadData { get; set; }

    public TLoadData? CategoryLoadData { get; set; }

    public PathFormatType ProductPathFormatType { get; set; }

    public PathFormatType CategoryPathFormatType { get; set; }
}