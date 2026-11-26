using Import.Catalog.Product.Interfaces;
using Import.Interfaces;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Factory;

internal class ProductServiceImpl<TLoadOptions, TCategory, TProductItem> : ProductService<TLoadOptions, TCategory, TProductItem>
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
    where TLoadOptions : class
{
    public override string Name { get; }

    internal ProductServiceImpl(
        ILogger<ProductServiceImpl<TLoadOptions, TCategory, TProductItem>> logger,
        string name,
        ILoaderService<LoadData<TLoadOptions>> loader,
        string url,
        IEnumerable<IProductCatalogCategory> catalogCategories,
        IProductItemHandler productItemHandler,
        string productPathFormat,
        string categoryPathFormat,
        string sourceName,
        ICategoryPaging<TCategory> categoryPaging,
        IItemSerializer<TCategory> categorySerializer,
        ProductServiceOptions<TLoadOptions> productServiceOptions)
        : base(logger,
        loader,
        url,
        catalogCategories,
        productItemHandler,
        productPathFormat,
        categoryPathFormat,
        sourceName,
        categoryPaging,
        categorySerializer,
        productServiceOptions)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        Name = name;
    }

    protected override string PreparePath(string path) => path;
}