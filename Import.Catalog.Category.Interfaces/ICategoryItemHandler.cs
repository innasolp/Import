using Import.Interfaces;

namespace Import.Catalog.Category.Interfaces;

public interface IImportCategory<TCategory>
{
    TCategory Category { get; }

    string CategorySourceUrl { get; }

    string SourceName { get; }

    string SourceUrl { get; }
}

public interface ICategoryItemHandler<TCategory> : IItemHandler<IImportCategory<TCategory>, ResultStatus>
{
}