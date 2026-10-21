using System.Collections.Concurrent;
using System.Text.Json;

namespace Import.TreeItem.Json;

public class JsonTreeItemLoader<TTreeItem>(JsonTreeItemLoadOptions jsonTreeItemLoadOptions) : ITreeItemLoader<TTreeItem>
    where TTreeItem : TreeItem, new()
{
    public bool IsTreeItem { get; } = jsonTreeItemLoadOptions.IsTree;

    public JsonTreeItemLoadOptions JsonTreeItemLoadOptions { get; } = jsonTreeItemLoadOptions;

    ITreeItemLoadOptions ITreeItemLoader<TTreeItem>.TreeItemLoadOptions => JsonTreeItemLoadOptions;

    public static async Task<IEnumerable<TTreeItem>> LoadRootElementAsync(Stream stream, JsonTreeItemLoadOptions treeItemLoadOptions, CancellationToken cancellationToken = default)
    {
        var jsonDocument = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var elementHelper = new JsonElementHelper();
        var categories = new ConcurrentBag<TTreeItem>();
        await TreeItem.LoadAllChildrenAsync(null, categories, jsonDocument.RootElement,
                treeItemLoadOptions.FirstNodePath,
                treeItemLoadOptions.PropertyPaths,
                elementHelper,
                cancellationToken);

        return categories;
    }

    Task<IEnumerable<TTreeItem>> ITreeItemLoader<TTreeItem>.LoadAsync(TTreeItem? parent, Stream stream, CancellationToken cancellationToken)
    {
        return LoadRootElementAsync(stream, JsonTreeItemLoadOptions, cancellationToken);
    }
}