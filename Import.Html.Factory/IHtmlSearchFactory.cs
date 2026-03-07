namespace Import.Html.Factory;

public interface IHtmlSearchFactory
{
    IHtmlSearcher CreateSearcher(SearchMatchType searchType, SearchElementType searchElementType);
}
