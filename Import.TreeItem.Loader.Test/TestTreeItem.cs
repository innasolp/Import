using System.Collections;

namespace Import.TreeItem.Loader.Test;

public class TestTreeItem : TreeItem
{
    public TestTreeItem()
    {
    }

    public List<TestTreeItem> ChildrentItems { get; } = [];

    protected override IList Children => ChildrentItems;

    public TestTreeItem? TestItemParent { get; protected set; }

    protected override TreeItem? ItemParent { get => TestItemParent; set => TestItemParent = value as TestTreeItem; }

    protected override void SetProperties<TElement>(TElement element, IReadOnlyDictionary<string, PropertyPath> propertyPaths, IElementHelper<TElement> elementHelper)
    {
    }
}