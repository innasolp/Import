using System.Text.Json;

namespace Import.KeyHash;

public static class JsonCanonicalizer
{
    private static readonly JsonSerializerOptions MinifiedOptions = new() { WriteIndented = false };

    public static byte[] GetCanonicalJson<T>(T value)
    {
        var json = JsonSerializer.Serialize(value, MinifiedOptions);

        using var doc = JsonDocument.Parse(json);

        using var ms = new MemoryStream();
        using (var writer = new Utf8JsonWriter(ms, new JsonWriterOptions { Indented = false }))
        {
            WriteCanonical(doc.RootElement, writer);
        }

        return ms.ToArray();
    }

    private static void WriteCanonical(JsonElement element, Utf8JsonWriter writer)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var prop in element.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(prop.Name);
                    WriteCanonical(prop.Value, writer);
                }
                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                    WriteCanonical(item, writer);
                writer.WriteEndArray();
                break;

            default:
                element.WriteTo(writer);
                break;
        }
    }
}