namespace Import.Catalog.Category.Interfaces;

public class Category : ICategory
{
    public string? ParentName { get; init; }
    public int? ParentItemId { get; init; }
    public string? ParentItemUrl { get; init; }
    public string Name { get; init; }
    public int ItemId { get; init; }
    public string Url { get; init; }
}