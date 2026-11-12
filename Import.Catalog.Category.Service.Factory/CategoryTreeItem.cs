using System.Collections;
using Import.TreeItem;

namespace Import.Catalog.Category.Service.Factory;

public class CategoryTreeItem : TreeItem.TreeItem
{
    private readonly List<CategoryTreeItem> _children = [];

    protected override IList Children => _children;

    private CategoryTreeItem? _parent;

    protected override TreeItem.TreeItem? ItemParent { get => _parent; set => _parent = value as CategoryTreeItem; }

    public string? Description { get; private set; }

    protected override void SetProperties<TElement>(TElement element, IReadOnlyDictionary<string, PropertyPath> propertyPaths, IElementHelper<TElement> elementHelper)
    {
        if (propertyPaths.TryGetValue(nameof(Description), out var descriptionPath)
            && TryGetProperty(element, descriptionPath, elementHelper.GetString, elementHelper, out var description))
        {
            Description = description;
        }
    }
}