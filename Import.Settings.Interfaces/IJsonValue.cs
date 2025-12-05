using System.Text.Json;
using System.Text.Json.Nodes;


namespace Import.Settings.Interfaces;

public interface IJsonValue
{
    JsonElement? ValueObj { get; set; }
    JsonObject? Value { get; set; }
}
