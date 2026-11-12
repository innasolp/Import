namespace Import.Catalog.Category.Interfaces;

public interface ICategory
{
    string? ParentName { get; init; }

    int? ParentItemId { get; init; }

    string? ParentItemUrl { get; init; }

    string Name { get; init; }

    int ItemId { get; init; }

    string Url { get; init; }
}