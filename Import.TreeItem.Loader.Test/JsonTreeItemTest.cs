using Import.TreeItem.Json;
using Xunit.Abstractions;

namespace Import.TreeItem.Loader.Test;

public class JsonTreeItemTest(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Fact]
    public async Task TestOzonJsonCategoriesLoading()
    {
        string path = "ozon.category.9200.json";

        string[] nodePath = ["data", "columns", "categories"];

        var propertyPaths = new Dictionary<string, PropertyPath>
        {
            { "Id", new PropertyPath("Id","id") },
            { "Url", new PropertyPath("Url","url") },
            {"Description",new PropertyPath("Description","title") },
            {"Name",new PropertyPath("Name","title") },
            {"Children",new PropertyPath("Children","categories") }
        };
        ITreeItemLoader<TestTreeItem> jsonTreeItemLoader = new JsonTreeItemLoader<TestTreeItem>(new JsonTreeItemLoadOptions
        {
            PropertyPaths = propertyPaths,
            FirstNodePath = nodePath
        });

        using FileStream fileStream = File.OpenRead(path);

        var treeItems = await jsonTreeItemLoader.LoadAsync(null, fileStream);

        fileStream.Close();

        Assert.NotEmpty(treeItems);

        var firstItem = treeItems.First();
        Assert.NotEmpty(firstItem.Name);
        Assert.NotEmpty(firstItem.Url);
        Assert.True(firstItem.Id > 0);

        var parentCount = treeItems.Count(i => i.Children.Any());
        var parentAndWithParentCount = treeItems.Count(i => i.ItemParent != null && i.Children.Any());
        try
        {
            Assert.True(parentCount > 1);
            Assert.Equal(0, parentAndWithParentCount);
        }
        finally
        {
            _testOutputHelper.WriteLine($"parentCount {parentCount}");
            _testOutputHelper.WriteLine($"parentAndWithParentCount {parentAndWithParentCount}");
        }
    }

    [Fact]
    public async Task TestGoldAppleJsonCategoriesLoading()
    {
        string path = "goldapple.categories.json";

        string[] nodePath = ["general"];

        var propertyPaths = new Dictionary<string, PropertyPath>()
        {
            { "Url",new PropertyPath("Url","link",true) },
            {"Description",new PropertyPath("Description","name") },
            {"Name",new PropertyPath("Name","name") },
            {"Id",new PropertyPath("Id","id") },
            {"Children",new PropertyPath("Children","children") }
        };
        ITreeItemLoader<TestTreeItem> jsonTreeItemLoader = new JsonTreeItemLoader<TestTreeItem>(new JsonTreeItemLoadOptions
        {
            PropertyPaths = propertyPaths,
            FirstNodePath = nodePath
        });

        using FileStream fileStream = File.OpenRead(path);

        var treeItems = await jsonTreeItemLoader.LoadAsync(null, fileStream);

        fileStream.Close();

        Assert.NotEmpty(treeItems);

        var firstItem = treeItems.First();
        Assert.NotEmpty(firstItem.Name);
        Assert.NotEmpty(firstItem.Url);
        Assert.True(firstItem.Id > 0);

        var parentCount = treeItems.Count(i => i.Children.Any());
        var parentAndWithParentCount = treeItems.Count(i => i.ItemParent != null && i.Children.Any());
        try
        {
            Assert.True(parentCount > 1);
            Assert.True(parentAndWithParentCount > 1);
        }
        finally
        {
            _testOutputHelper.WriteLine($"parentCount {parentCount}");
            _testOutputHelper.WriteLine($"parentAndWithParentCount {parentAndWithParentCount}");
        }
    }
}