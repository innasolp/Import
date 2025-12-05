using AngleSharp.Html.Dom;

namespace Import.Html.Factory;

internal class EqualsAttributeHtmlSearcher : HtmlSearcher
{
    protected override List<string?> GetAllValuesFromDocument(IHtmlDocument document, HtmlSearchOptions htmlSearchOptions)
    {
        return document.QuerySelectorAll($"{htmlSearchOptions.Tag}/{htmlSearchOptions.SearchString}")
            .Select(e => e.GetAttribute(htmlSearchOptions.ValueString)).ToList();
    }

}
