namespace Import.Settings.Interfaces;

public interface ISplittableImportSource<TSource> : IImportSource
    where TSource : IImportSource
{
    IEnumerable<TSource> Split();
}

public interface ISplittableImportSource : IImportSource
{
    IEnumerable<IImportSource> Split();
}