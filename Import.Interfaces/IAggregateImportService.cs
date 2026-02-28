namespace Import.Interfaces;

public interface IAggregateImportService : IImportService
{
    IEnumerable<IImportService> GetImportServices();

    Task Enqueue(IImportService service, CancellationToken cancellationToken = default);

    Task Dequeue(IImportService service, CancellationToken cancellationToken = default);
}
