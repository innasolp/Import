using System.Text.Json.Serialization;

namespace Import.Catalog.Product.Interfaces;

[JsonConverter(typeof(JsonStringEnumConverter<PathFormatType>))]
public enum PathFormatType
{
    None = 0,
    Path = 1,
    ItemId = 2,
    CategoryWithItemId = 3
}