using Import.Catalog.Product.Interfaces;

namespace Import.Catalog.Product.Service;

public abstract class ProductItemPathFormatter<TProductItem> : IItemPathFormatter<TProductItem>
where TProductItem : ICategoryProductItem
{
    public string GetAbsolutePath(string pathFormat, PathFormatType pathFormatType, TProductItem productItem)
    {
        return pathFormatType switch
        {
            PathFormatType.ItemId => string.Format(pathFormat, productItem.Id),
            PathFormatType.Path => string.Format(pathFormat, PrepareItemPath(productItem.ItemPath)),
            PathFormatType.CategoryWithItemId => string.Format(pathFormat, productItem.Id, PrepareItemPath(productItem.ItemPath)),
            _ => string.Format(pathFormat, productItem.ItemPath),
        };
    }
    
    public abstract string PrepareItemPath(string itemPath);
}