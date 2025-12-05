namespace Import.Html.Factory;

public class HtmlSearchFactory 
{
    public static IHtmlSearcher CreateSearcher(SearchMatchType? searchMatchType, SearchElementType? searchElementType = SearchElementType.Attribute)
    {
        if (searchElementType == SearchElementType.Attribute)
            switch (searchMatchType)
            {
                case SearchMatchType.Equals:
                    return new EqualsAttributeHtmlSearcher();

                case SearchMatchType.Like:
                    return new LikeAttributeHtmlSearcher();

                default:
                    return new EmptySearcher();
            }
        else
            switch (searchMatchType)
            {
                case SearchMatchType.Equals:
                    return new EqualsByJsonValueHtmlSearcher();

                case SearchMatchType.Like:
                    return new LikeAttributeHtmlSearcher();

                default:
                    return new EmptySearcher();
            }
    }
}
