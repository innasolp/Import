using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace Import.TreeItem;

public abstract class TreeItem : ITreeItem
{
    public string Name { get; private init; } = string.Empty;

    public string Url { get; private init; } = string.Empty;

    public int Id { get; private init; }

    public int? ParentId { get; protected set; }

    private readonly List<ITreeItem> _children = [];

    [JsonIgnore]
    public IEnumerable<ITreeItem> Children => _children;

    public bool? IsParented { get; protected set; }

    public ITreeItem? ItemParent { get; protected set; }

    protected abstract void SetProperties<TElement>(
        TElement element,
        IReadOnlyDictionary<string, PropertyPath> propertyPaths,
        IElementHelper<TElement> elementHelper);

    private static bool TryGetTreeItemFrom<TElement, TTreeItem>(
        TElement element,
        IReadOnlyDictionary<string, PropertyPath> propertyPaths,
        IElementHelper<TElement> elementHelper,
        out TTreeItem? treeItem)
        where TTreeItem : TreeItem, new()
    {
        treeItem = default;

        if (!propertyPaths.TryGetValue(nameof(Url), out var urlPath))
            return false;

        if (!TryGetProperty(element, urlPath, elementHelper.GetString, elementHelper, out var url)
            || string.IsNullOrEmpty(url))
        {
            url = string.Empty;
        }

        if ((!propertyPaths.TryGetValue(nameof(Id), out var idPath)
            || !TryGetProperty(element, idPath, elementHelper.GetInt32, elementHelper, out int id))
            && !Utils.TryGetIdFromUrl(url, out id))
        {
            return false;
        }

        if (!propertyPaths.TryGetValue(nameof(Name), out var namePath)
            || !TryGetProperty(element, namePath, elementHelper.GetString, elementHelper, out var name)
            || string.IsNullOrEmpty(name))
        {
            return false;
        }

        var item = new TTreeItem
        {
            Id = id,
            Name = name,
            Url = url
        };

        item.SetProperties(element, propertyPaths, elementHelper);

        if (propertyPaths.TryGetValue(nameof(IsParented), out var isParentedPath))
        {
            if (!TryGetProperty(element, isParentedPath, elementHelper.GetBoolean, elementHelper, out bool isParented))
                return false;

            item.IsParented = isParented;
        }

        treeItem = item;
        return true;
    }

    private static bool TryGetProperty<TElement, T>(
        TElement element,
        PropertyPath propertyPath,
        Func<TElement, T> getValue,
        IElementHelper<TElement> elementHelper,
        out T? result)
    {
        if (elementHelper.TryGetByProperty(element, propertyPath.Path, out TElement newElement))
        {
            try
            {
                result = getValue(newElement);
                return true;
            }
            catch (Exception ex) when (ex is InvalidOperationException or FormatException)
            {
                throw new InvalidOperationException($"Element parsing error {element}, path {propertyPath.Path}", ex);
            }
        }

        result = default;
        return false;
    }

    private static void LoadChildrenTree<TElement, TTreeItem>(
        TTreeItem? parent,
        ConcurrentBag<TTreeItem> treeItems,
        IEnumerable<TElement> childrenElements,
        IReadOnlyDictionary<string, PropertyPath> propertyPaths,
        PropertyPath? childrenPath,
        IElementHelper<TElement> elementHelper,
        CancellationToken cancellationToken)
        where TTreeItem : TreeItem, new()
    {
        foreach (var element in childrenElements)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!TryGetTreeItemFrom(element, propertyPaths, elementHelper, out TTreeItem? treeItem)
                || treeItem is null
                || treeItem.Id == parent?.Id)
            {
                continue;
            }

            if (parent?.Id > 0)
                SetParent(treeItem, parent);

            treeItems.Add(treeItem);

            if (childrenPath is not null
                && elementHelper.TryGetElementEnumerable(element, childrenPath.Path, out var newChildrenElements))
            {
                LoadChildrenTree(treeItem, treeItems, newChildrenElements, propertyPaths, childrenPath, elementHelper, cancellationToken);
            }
        }
    }

    protected static void SetParent(TreeItem treeItem, TreeItem parent)
    {
        treeItem.ParentId = parent.Id;
        treeItem.ItemParent = parent;

        lock (parent._children)
        {
            parent._children.Add(treeItem);
        }
    }

    protected static IEnumerable<TElement> GetAllElementsByNodePath<TElement>(
        TElement element,
        string[] nodePath,
        IElementHelper<TElement> elementHelper)
    {
        var results = new List<TElement>();
        GetElementsByAnyNodePathes(element, nodePath.AsSpan(), elementHelper, results);
        return results;
    }

    private static void GetElementsByAnyNodePathes<TElement>(
        TElement currentElement,
        ReadOnlySpan<string> remainingPathes,
        IElementHelper<TElement> elementHelper,
        List<TElement> results)
    {
        for (int i = 0; i < remainingPathes.Length; i++)
        {
            var path = remainingPathes[i];

            if (elementHelper.TryGetElementEnumerable(currentElement, path, out var innerElements))
            {
                var nextPath = remainingPathes[(i + 1)..];
                foreach (var innerElement in innerElements)
                {
                    if (!elementHelper.IsEmpty(innerElement))
                    {
                        GetElementsByAnyNodePathes(innerElement, nextPath, elementHelper, results);
                    }
                }
                return;
            }

            if (elementHelper.TryGetByProperty(currentElement, path, out var nextElement))
            {
                currentElement = nextElement;
            }
        }

        results.Add(currentElement);
    }

    public static async Task LoadAllChildrenAsync<TElement, TTreeItem>(
        TTreeItem? parent,
        ConcurrentBag<TTreeItem> treeItems,
        TElement element,
        string[]? nodePath,
        IReadOnlyDictionary<string, PropertyPath> propertyPaths,
        IElementHelper<TElement> elementHelper,
        CancellationToken cancellationToken)
        where TTreeItem : TreeItem, new()
    {
        await Task.Run(() =>
        {
            var rootElements = nodePath is { Length: > 0 }
                ? GetAllElementsByNodePath(element, nodePath, elementHelper)
                : [element];

            propertyPaths.TryGetValue(nameof(Children), out var childrenPath);

            Parallel.ForEach(rootElements, new ParallelOptions { CancellationToken = cancellationToken }, rootElement =>
            {
                if (elementHelper.TryGetElementEnumerable(rootElement, null, out var childElements))
                {
                    LoadChildrenTree(parent, treeItems, childElements, propertyPaths, childrenPath, elementHelper, cancellationToken);
                }
                else
                {
                    if (!TryGetTreeItemFrom<TElement, TTreeItem>(rootElement, propertyPaths, elementHelper, out var treeItem)
                        || treeItem is null)
                    {
                        return;
                    }

                    if (parent?.Id > 0)
                        SetParent(treeItem, parent);

                    treeItems.Add(treeItem);

                    if (childrenPath is not null
                        && elementHelper.TryGetElementEnumerable(rootElement, childrenPath.Path, out childElements))
                    {
                        LoadChildrenTree(treeItem, treeItems, childElements, propertyPaths, childrenPath, elementHelper, cancellationToken);
                    }
                }
            });
        }, cancellationToken).ConfigureAwait(false);
    }
}