using System.Text.Json;
using System.Text.Json.Nodes;

namespace Import.Settings.Interfaces;

public interface IJsonNodeValue
{
    JsonNode? Value { get; set; }

    JsonElement? ValueObj { get; set; }
}