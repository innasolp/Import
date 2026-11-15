namespace Import.TreeItem.Json;

public class JsonTreeItemLoadOptions : ITreeItemLoadOptions
{
    public Dictionary<string, PropertyPath> PropertyPaths { get; set; }

    public string? UrlFormat { get; set; }

    public string[] FirstNodePath { get; set; }

    public bool IsTree { get; set; } = false;
}