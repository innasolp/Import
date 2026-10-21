using System.Text.Json;
using System.Text.Json.Nodes;

namespace Import.TreeItem.Json;

internal class JsonTreeItemLoaderFactory<TTreeItem> : ITreeItemLoaderFactory<TTreeItem>
    where TTreeItem : TreeItem, new()
{
    ITreeItemLoader<TTreeItem> ITreeItemLoaderFactory<TTreeItem>.CreateTreeItemLoader(ITreeItemLoadOptions treeItemLoadOptions)
    {
        if (treeItemLoadOptions is not JsonTreeItemLoadOptions jsonTreeItemLoadOptions)
            throw new ArgumentException($"Expected {nameof(JsonTreeItemLoadOptions)} but received {treeItemLoadOptions.GetType().Name}");

        return CreateJsonTreeItemLoader(jsonTreeItemLoadOptions);
    }

    public static JsonTreeItemLoader<TTreeItem> CreateJsonTreeItemLoader(JsonTreeItemLoadOptions treeItemLoadOptions)
    {
        return new JsonTreeItemLoader<TTreeItem>(treeItemLoadOptions);
    }

    ITreeItemLoader<TTreeItem> ITreeItemLoaderFactory<TTreeItem>.CreateTreeItemLoader(JsonObject treeItemLoadOptions)
    {
        return CreateTreeItemLoader(treeItemLoadOptions);
    }

    public static JsonTreeItemLoader<TTreeItem> CreateTreeItemLoader(JsonObject treeItemLoadOptionsJson)
    {
        var treeItemLoadOptions = JsonSerializer.Deserialize<JsonTreeItemLoadOptions>(treeItemLoadOptionsJson.ToJsonString());
        return treeItemLoadOptions == null
            ? throw new ArgumentException($"Failed to deserialize {nameof(JsonTreeItemLoadOptions)} from JsonObject")
            : CreateJsonTreeItemLoader(treeItemLoadOptions);
    }
}