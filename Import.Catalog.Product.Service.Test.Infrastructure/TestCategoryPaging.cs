using Import.Catalog.Product.Interfaces;

namespace Import.Catalog.Product.Service.Test.Infrastructure;

public class TestCategoryPaging<TCategory> : CategoryPaging<TCategory>
     where TCategory : class, ICategoryProducts, new()
{
    protected override string PreparePath(string path) => path;
}