using Import.Catalog.Product.Interfaces;
using System.Text.Json.Serialization;

namespace Import.Catalog.Product.Service.Stateful;

internal class ServiceState<TCategory>
    where TCategory : class, ICategoryProducts, new()
{
    [JsonInclude]
    public CategoryProcessState CategoryProcessState { get; private set; } = CategoryProcessState.Start();

    [JsonInclude]
    public TCategory? Category { get; set; }

    [JsonInclude]
    public ProductCatalogCategory? ProductCatalogCategory { get; private set; }

    [JsonInclude]
    public string? CurrentCategoryProductItemId { get; set; }

    [JsonInclude]
    public HashSet<string> HandledCategoryProductItemIds { get; private set; } = [];

    public void Reset()
    {
        CategoryProcessState.Reset();
        Category = null;
        ProductCatalogCategory = null;
        CurrentCategoryProductItemId = null;
        HandledCategoryProductItemIds.Clear();
    }

    public void Start(IProductCatalogCategory productCatalogCategory)
    {
        ProductCatalogCategory = new ProductCatalogCategory
        {
            Path = productCatalogCategory.Path,
            Category = productCatalogCategory.Category,
            ItemId = productCatalogCategory.ItemId
        };

        CategoryProcessState = CategoryProcessState.Start(ProductCatalogCategory.Path);
    }
}