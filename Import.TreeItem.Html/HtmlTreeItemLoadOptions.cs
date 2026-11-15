using Import.Html;
using Import.Html.Factory;

namespace Import.TreeItem.Html;

public class HtmlTreeItemLoadOptions : ITreeItemLoadOptions
{
    public HtmlSearchOptions HtmlSearchOptions { get; set; }

    public HtmlSearchFactoryOptions HtmlSearchFactoryOptions { get; set; }

    public Dictionary<string, PropertyPath> PropertyPaths { get; set; }

    public string? UrlFormat { get; set; }

    public string[] FirstNodePath { get; set; }

    public bool IsTree { get; set; } = false;
}
