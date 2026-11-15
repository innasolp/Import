using Import.Interfaces;

namespace Import.Catalog.Product.Interfaces;

public interface IImportProduct
{
    IProductItem ProductItem { get; }

    Stream Stream { get; }

    string SourceName { get; }

    string SourcePath { get; }
}

public interface IProductItemHandler : IItemHandler<IImportProduct, ResultStatus>
{
}
