using Import.Html;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Import.TreeItem.Html;

public class TreeItemJsonFromHtmlLoader<TTreeItem>(IHtmlSearcher htmlSearcher, HtmlTreeItemLoadOptions treeItemLoadOptions)
    : ITreeItemLoader<TTreeItem>
    where TTreeItem : TreeItem, new()

{
    public IHtmlSearcher HtmlSearcher { get; } = htmlSearcher;

    public bool IsTreeItem { get; } = treeItemLoadOptions.IsTree;

    public HtmlTreeItemLoadOptions HtmlTreeItemLoadOptions { get; } = treeItemLoadOptions;

    ITreeItemLoadOptions ITreeItemLoader<TTreeItem>.TreeItemLoadOptions => HtmlTreeItemLoadOptions;

    async Task<IEnumerable<TTreeItem>> ITreeItemLoader<TTreeItem>.LoadAsync(TTreeItem? parent, Stream stream, CancellationToken cancellationToken)
    {
        return await LoadRootElementAsync(parent, stream, cancellationToken);
    }

    public async Task<IEnumerable<TTreeItem>> LoadRootElementAsync(TTreeItem? parent,
        Stream stream,
         CancellationToken cancellationToken = default)
    {
        var values = await HtmlSearcher.GetValues(stream, HtmlTreeItemLoadOptions.HtmlSearchOptions, cancellationToken);
        if (values.Count == 0)
            return [];

        var rootElement = JsonDocument.Parse(values[0]).RootElement;

        var elementHelper = new JsonElementHelper();
        var treeItems = new ConcurrentBag<TTreeItem>();
        await TreeItem.LoadAllChildrenAsync(parent: parent,
        treeItems: treeItems,
        element: rootElement,
                nodePath: HtmlTreeItemLoadOptions.FirstNodePath,
                 propertyPaths: HtmlTreeItemLoadOptions.PropertyPaths,
                elementHelper: elementHelper,
                cancellationToken: cancellationToken);

        return treeItems;
    }
}