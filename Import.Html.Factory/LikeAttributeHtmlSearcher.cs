using AngleSharp.Html.Dom;

namespace Import.Html.Factory;

internal class LikeAttributeHtmlSearcher : HtmlSearcher
{
    protected override List<string?> GetAllValuesFromDocument(IHtmlDocument document, HtmlSearchOptions htmlSearchOptions)
    {
        var values = new List<string?>();

        var split = htmlSearchOptions.SearchString?.Split("=");
        if (split == null || split.Length < 2 && split.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException($"Search html pattern {htmlSearchOptions.SearchString} is invalid");
        var searchAttr = split[0];
        var searchVal = split[1];
        foreach (var e in document.QuerySelectorAll(htmlSearchOptions.Tag))
        {
            var attr = e.GetAttribute(searchAttr);
            if (attr == null) continue;
            if (attr.Contains(searchVal))
                values.Add(e.GetAttribute(htmlSearchOptions.ValueString));
        }
        return values;
    }
}
