using System.Collections;
using System.Collections.Specialized;
using Import.TreeItem;

namespace Import.Catalog.Category.Service.Test;

public class TestCategory : ITreeItem
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Url { get; set; }

    public int? ParentId { get; set; }

    public bool? IsParented { get; set; }

    public List<TestCategory> Children { get; set; } = [];

    ICollection ITreeItem.Children => Children;

    public List<int> ChildrenIds { get; set; } = [];

    public TestCategory? ItemParent { get; set; }

    ITreeItem? ITreeItem.ItemParent => ItemParent;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
}
