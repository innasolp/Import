namespace Import.TreeItem;

public interface ITreeItemLoader<TTreeItem>
    where TTreeItem : ITreeItem
{
    ITreeItemLoadOptions TreeItemLoadOptions { get; }

    Task<IEnumerable<TTreeItem>> LoadAsync(TTreeItem? parent, Stream stream, CancellationToken cancellationToken = default);

    public bool IsTreeItem { get; }
}