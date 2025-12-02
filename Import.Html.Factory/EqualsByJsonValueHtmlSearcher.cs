using AngleSharp.Html.Dom;

namespace Import.Html.Factory;

internal class EqualsByJsonValueHtmlSearcher : HtmlSearcher
{
    private readonly string valuesSeparator = ";";
    private readonly string innerSeparator = "=";
    protected override List<string?> GetAllValuesFromDocument(IHtmlDocument document, HtmlSearchOptions htmlSearchOptions)
    {
        var fullSearchString = htmlSearchOptions.Tag +
            (!string.IsNullOrEmpty(htmlSearchOptions.SearchString) ? $"/{htmlSearchOptions.SearchString}" : "");
        var elements = document.QuerySelectorAll(fullSearchString);
        var values = new List<string?>();
        foreach (var element in elements.Where(e => e.InnerHtml != null))
        {
            if (element.InnerHtml.Contains(htmlSearchOptions.ValueString))
            {
                var valueExpression = element.InnerHtml.Split(valuesSeparator).FirstOrDefault(v => v.Contains(htmlSearchOptions.ValueString));
                if (valueExpression == null) continue;
                var value = valueExpression.Replace($"{htmlSearchOptions.ValueString}{innerSeparator}", "");
                values.Add(value);
            }
        }
        return values;
    }
}
