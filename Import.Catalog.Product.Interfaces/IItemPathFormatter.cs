namespace Import.Catalog.Product.Interfaces;

public interface IItemPathFormatter<TItem>
{
    string GetAbsolutePath(string pathFormat, PathFormatType pathFormatType, TItem item);
}