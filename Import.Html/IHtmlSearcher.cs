namespace Import.Html;

public interface IHtmlSearcher
{
    Task<List<string>> GetValues(Stream html, HtmlSearchOptions htmlSearchOptions, CancellationToken stoppingToken);
}
