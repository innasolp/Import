namespace Import.TreeItem;

public class PropertyPath(string propertyName, string path, bool? stopLoadIfNotExists = false)
{
    public string PropertyName { get; set; } = propertyName;
    public string Path { get; set; } = path;
    public bool? StopLoadIfNotExists { get; set; } = stopLoadIfNotExists;
}