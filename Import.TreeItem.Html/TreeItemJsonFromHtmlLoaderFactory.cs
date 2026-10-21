using Import.Html.Factory;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace Import.TreeItem.Html;

internal class TreeItemJsonFromHtmlLoaderFactory<TTreeItem> : ITreeItemLoaderFactory<TTreeItem>
    where TTreeItem : TreeItem, new()
{
    ITreeItemLoader<TTreeItem> ITreeItemLoaderFactory<TTreeItem>.CreateTreeItemLoader(ITreeItemLoadOptions treeItemLoadOptions)
    {
        if (treeItemLoadOptions is not HtmlTreeItemLoadOptions htmlTreeItemLoadOptions)
            throw new ArgumentException($"Expected {nameof(HtmlTreeItemLoadOptions)} but got {treeItemLoadOptions.GetType().Name}");

        return CreateTreeItemLoader(htmlTreeItemLoadOptions);
    }

    public static TreeItemJsonFromHtmlLoader<TTreeItem> CreateTreeItemLoader(HtmlTreeItemLoadOptions treeItemLoadOptions)

    {
        var htmlSearcher = HtmlSearchFactory.CreateSearcher(treeItemLoadOptions.HtmlSearchFactoryOptions.SearchMatchType,
            treeItemLoadOptions.HtmlSearchFactoryOptions.SearchElementType);

        return new TreeItemJsonFromHtmlLoader<TTreeItem>(htmlSearcher, treeItemLoadOptions);
    }

    ITreeItemLoader<TTreeItem> ITreeItemLoaderFactory<TTreeItem>.CreateTreeItemLoader(JsonObject treeItemLoadOptions)
    {
        return CreateTreeItemLoader(treeItemLoadOptions);
    }

    public static TreeItemJsonFromHtmlLoader<TTreeItem> CreateTreeItemLoader(JsonObject treeItemLoadOptionsJson)
    {
        var treeItemLoadOptions = JsonSerializer.Deserialize<HtmlTreeItemLoadOptions>(treeItemLoadOptionsJson.ToJsonString());
        return treeItemLoadOptions == null
            ? throw new ArgumentException($"Failed to deserialize {nameof(HtmlTreeItemLoadOptions)} from JsonObject")
            : CreateTreeItemLoader(treeItemLoadOptions);
    }
}