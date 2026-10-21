using System.Text.Json;

namespace Import.TreeItem;

public class JsonElementHelper : IElementHelper<JsonElement>
{
    public bool GetBoolean(JsonElement element)
    {
        return element.GetBoolean();
    }

    public int GetInt32(JsonElement element)
    {
        return element.GetInt32();
    }

    public string GetRawText(JsonElement element)
    {
        return element.GetRawText();
    }

    public string? GetString(JsonElement element)
    {
        return element.GetString();
    }

    public bool IsEmpty(JsonElement element)
    {
        return !element.EnumerateObject().Any();
    }

    public bool TryGetByProperty(JsonElement element, string property, out JsonElement result)
    {
        return element.TryGetProperty(property, out result);
    }

    public bool TryGetElementEnumerable(JsonElement element, string? property, out IEnumerable<JsonElement>? result)
    {
        result = string.IsNullOrEmpty(property) && element.ValueKind == JsonValueKind.Array
            ? element.EnumerateArray()
            : !string.IsNullOrEmpty(property) && element.TryGetProperty(property, out var jsonArray) && jsonArray.ValueKind == JsonValueKind.Array
             ? jsonArray.EnumerateArray() : null;

        return result is not null;
    }
}