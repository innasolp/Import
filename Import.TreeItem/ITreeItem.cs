namespace Import.TreeItem;

public interface ITreeItem
{
    string Name { get; }

    string Url { get; }

    int Id { get; }

    int? ParentId { get; }

    IEnumerable<ITreeItem> Children { get; }

    public bool? IsParented { get; }

    public ITreeItem? ItemParent { get; }
}
