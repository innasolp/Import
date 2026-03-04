namespace Import.Interfaces;

public interface IAggregateImportService : IImportService
{
    IEnumerable<(IImportService Service, bool Completed, bool? Success)> GetImportServices();

    Task Enqueue(IImportService service, CancellationToken cancellationToken = default);

    Task<bool> TryRemove(IImportService service, CancellationToken cancellationToken = default);
}