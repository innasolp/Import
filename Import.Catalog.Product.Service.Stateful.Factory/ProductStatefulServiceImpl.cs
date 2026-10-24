using Import.Catalog.Product.Interfaces;
using Import.Catalog.Product.Service.Stateful;
using Import.Interfaces;
using Import.KeyHash;
using Import.ServiceState;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Product.Service.Stateful.Factory;

internal class ProductStatefulServiceImpl<TCategory, TProductItem> : ProductsStatefulService<TCategory, TProductItem>
    where TCategory : class, ICategoryProducts, new()
    where TProductItem : class, IProductItem, new()
{
    public override string Name { get; }

    internal ProductStatefulServiceImpl(
        ILogger logger,
        string name,
        ILoaderService loader,
        string url,
        IEnumerable<IProductCatalogCategory> catalogCategories,
        IProductItemHandler productItemHandler,
        string productPathFormat,
        string categoryPathFormat,
        string sourceName,
        ICategoryPaging<TCategory> categoryPaging,
        IItemSerializer<TCategory> categorySerializer,
        ProductServiceOptions productServiceOptions,
        IKeyHasher keyHasher,
        IServiceStateRepository serviceStateRepository)
        : base(logger, loader, url, catalogCategories, productItemHandler, productPathFormat, categoryPathFormat, sourceName, categoryPaging, categorySerializer, productServiceOptions, keyHasher, serviceStateRepository)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        Name = name;
    }

    protected override string PreparePath(string path) => path;
}
