using System.Text.Json.Nodes;

namespace Import.TreeItem;

public interface ITreeItemLoaderFactory<TTreeItem>
    where TTreeItem : ITreeItem
{
    ITreeItemLoader<TTreeItem> CreateTreeItemLoader(ITreeItemLoadOptions treeItemLoadOptions);

    ITreeItemLoader<TTreeItem> CreateTreeItemLoader(JsonObject treeItemLoadOptions);
}