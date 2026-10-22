namespace Import.Catalog.Product.Interfaces;

public interface IPagingItem
{
    string? GetPageUrl(string urlFormat, string item, int page);

    string? GetNextPageUrl(string urlFormat, string item, int page);
}
