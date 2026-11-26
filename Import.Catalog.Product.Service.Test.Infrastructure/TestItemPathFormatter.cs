using Import.Catalog.Product.Interfaces;

namespace Import.Catalog.Product.Service.Test.Infrastructure;

public class TestItemPathFormatter : IItemPathFormatter<ICategoryProductItem>
{
    public string GetAbsolutePath(string pathFormat, PathFormatType pathFormatType, ICategoryProductItem item)
    {
        return pathFormatType switch
        {
            PathFormatType.ItemId => string.Format(pathFormat, item.Id),
            PathFormatType.Path => string.Format(pathFormat, item.ItemPath),
            PathFormatType.CategoryWithItemId => string.Format(pathFormat, item.Id, item.ItemPath),
            _ => string.Format(pathFormat, item.ItemPath),
        };
    }
}
