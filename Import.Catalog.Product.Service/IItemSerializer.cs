namespace Import.Catalog.Product.Service;

public interface IItemSerializer<TItem>
{
    Task<TItem?> DeserializeFromStream(Stream stream, CancellationToken cancellationToken = default);
}