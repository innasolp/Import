namespace Import.Html.Factory;

internal class EmptySearcher : IHtmlSearcher
{
    public Task<List<string>> GetValues(Stream html, HtmlSearchOptions htmlSearchOptions, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}
