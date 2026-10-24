using System.Collections;
using Import.Catalog.Category.Interfaces;
using Import.Interfaces;
using Import.TreeItem;
using Microsoft.Extensions.Logging;

namespace Import.Catalog.Category.Service.Factory;

internal class Category : TreeItem.TreeItem
{
    private readonly List<Category> _children = [];

    protected override IList Children => _children;

    private Category? _parent;

    protected override TreeItem.TreeItem? ItemParent { get => _parent; set => _parent = value as Category; }

    public string? Description { get; private set; }

    protected override void SetProperties<TElement>(TElement element, IReadOnlyDictionary<string, PropertyPath> propertyPaths, IElementHelper<TElement> elementHelper)
    {
        if (propertyPaths.TryGetValue(nameof(Description), out var descriptionPath)
            && TryGetProperty(element, descriptionPath, elementHelper.GetString, elementHelper, out var description))
        {
            Description = description;
        }
    }
}

internal class CategoryServiceImpl<TTreeItem>(ILogger logger,
string name,
 ILoaderService loader,
  string categorySourceUrl,
  string sourceName,
   string url,
   IEnumerable<ITreeItemLoader<TTreeItem>> categoryLoadStages,
   ICategoryItemHandler<TTreeItem> itemHandler,
   object? categoryLoadData = null) : CategoryService<TTreeItem>(logger, name, loader, categorySourceUrl, sourceName, url, categoryLoadStages, itemHandler, categoryLoadData)
   where TTreeItem : TreeItem.TreeItem, new()
{
}
