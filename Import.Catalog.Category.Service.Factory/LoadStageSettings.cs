using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Import.Catalog.Category.Service.Factory;

public class LoadStageSettings
{
    [JsonPropertyName("Options")]
    public required JsonObject OptionsJson { get; set; }

    [JsonPropertyName("Type")]
    public required string TypeName { get; set; }
}