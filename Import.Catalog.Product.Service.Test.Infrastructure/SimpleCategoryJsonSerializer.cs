using Import.Catalog.Product.Interfaces;
using System.Text.Json;

namespace Import.Catalog.Product.Service.Test.Infrastructure;

public class SimpleCategoryJsonSerializer<TCategory> : IItemSerializer<TCategory>
    where TCategory : class, ICategoryProducts, new()
{
    public async Task<TCategory?> DeserializeFromStream(Stream stream, CancellationToken cancellationToken = default)
    {
        return await JsonSerializer.DeserializeAsync<TCategory>(stream, cancellationToken: cancellationToken);
    }
}