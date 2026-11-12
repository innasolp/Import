using Import.Interfaces;

namespace Import.Catalog.Category.Interfaces;

public interface IImportCategory<TCategory>
    where TCategory : ICategory
{
    TCategory Category { get; }

    string CategorySourceUrl { get; }

    string SourceName { get; }

    string SourceUrl { get; }
}

public interface ICategoryItemHandler<TCategory> : IItemHandler<IImportCategory<TCategory>, ResultStatus>
    where TCategory : ICategory
{
}