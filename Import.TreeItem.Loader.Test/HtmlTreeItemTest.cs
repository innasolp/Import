using Import.Html;
using Import.Html.Factory;
using Import.TreeItem.Html;

namespace Import.TreeItem.Loader.Test;

public class HtmlTreeItemTest
{
    private readonly string _path = "ozon.category.div.html";

    private readonly string[] _ozonNodePath = ["data", "columns", "categories"];

    private readonly Dictionary<string, PropertyPath> _ozonPropertyPaths = new()
    {
        { "Url", new PropertyPath("Url","url") },
        {"Description",new PropertyPath("Description","title") },
        {"Name",new PropertyPath("Name","title") },
        {"Children",new PropertyPath("Children","categories") }
    };

    [Fact]
    public async Task TestHtmlCategoriesLoading()
    {
        var htmlSearchOptions = new HtmlSearchOptions
        {
            Tag = "div",
            SearchString = "id=state-catalogMenu",
            ValueString = "data-state",
        };
        var options = new HtmlTreeItemLoadOptions()
        {
            HtmlSearchOptions = htmlSearchOptions,
            PropertyPaths = _ozonPropertyPaths,
            FirstNodePath = _ozonNodePath
        };
        var htmlSearcher = HtmlSearchFactory.CreateSearcher(SearchMatchType.Like);
        ITreeItemLoader<TestTreeItem> loader = new TreeItemJsonFromHtmlLoader<TestTreeItem>(htmlSearcher, options);

        using FileStream fileStream = File.OpenRead(_path);

        var treeItems = await loader.LoadAsync(null, fileStream);

        fileStream.Close();

        Assert.NotEmpty(treeItems);

        var firstItem = treeItems.First();
        Assert.NotEmpty(firstItem.Name);
        Assert.NotEmpty(firstItem.Url);
        Assert.True(firstItem.Id > 0);
        Assert.True(treeItems.All(i => (i as ITreeItem).Children.Count == 0));
    }
}
