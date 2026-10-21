
namespace Import.Catalog.Category.Service.Test;

public static class Helper
{
    public static TestCategory CreateCategory()
    {
        var rnd = new Random();
        return new TestCategory()
        {
            Name = $"Test_{Guid.NewGuid()}",
            Description = $"Description_{Guid.NewGuid()}",
            Url = $"https://{Guid.NewGuid()}",
            Id = rnd.Next(100000)
        };
    }

    public static TestCategory CreateCategoryWithChildren()
    {
        var parentCategory = CreateCategory();
        var childrenCount = new Random().Next(10);
        var children = new List<TestCategory>();
        for (int i = 0; i < childrenCount; i++)
        {
            var child = CreateCategory();
            child.ItemParent = parentCategory;
            child.ParentId = parentCategory.Id;
        }
        parentCategory.Children = children;
        parentCategory.ChildrenIds = [.. children.Select(c => c.Id)];
        return parentCategory;
    }
}
