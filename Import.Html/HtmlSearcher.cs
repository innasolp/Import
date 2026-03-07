using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace Import.Html;

public abstract class HtmlSearcher : IHtmlSearcher
{
    public virtual async Task<List<string>> GetValues(Stream html, HtmlSearchOptions htmlSearchOptions, CancellationToken stoppingToken)
    {
        List<string> tags = [];

        var parser = new HtmlParser();

        var document = await parser.ParseDocumentAsync(html, stoppingToken);

        var allValues = GetAllValuesFromDocument(document, htmlSearchOptions);
        allValues.ForEach(v =>
        {
            if (v != null) tags.Add(v);
        });

        return await Task.FromResult(tags);
    }

    protected abstract List<string?> GetAllValuesFromDocument(IHtmlDocument document, HtmlSearchOptions htmlSearchOptions);
}
