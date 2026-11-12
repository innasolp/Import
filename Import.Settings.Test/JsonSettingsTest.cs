using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Import.Settings.Interfaces;
using Import.Settings.Json;

namespace Import.Settings.Test;

public record OzonCategorySettings
{
    public string? Name { get; init; }
    public string? Caption { get; init; }
    public string? Url { get; init; }
    public string? CategorySourceUrl { get; init; }
    public Dictionary<string, object>? Services { get; init; }
}

public class LoadStageSettings
{
    [JsonPropertyName("Options")]
    public required JsonObject OptionsJson { get; set; }

    [JsonPropertyName("Type")]
    public required string TypeName { get; set; }
}

public class JsonSettingsTest
{
    private readonly string _jsonContent;

    public JsonSettingsTest()
    {
        _jsonContent = File.ReadAllText("appsettings.json");
    }

    [Fact]

    public void LoadSettingsFromJsonSuccessWhenJsonIsValid()
    {
        using JsonDocument doc = JsonDocument.Parse(_jsonContent);

        Assert.True(doc.RootElement.TryGetProperty("OzonCategories", out JsonElement sectionElement));
        var jsonObject = JsonObject.Create(sectionElement);
        Assert.NotNull(jsonObject);
        var importSettings = new ImportSettings(jsonObject);
        IImportSettingsProvider importSettingsProvider = new ImportSettingsProvider();

        Assert.True(importSettings.ServiceSettings.ContainsKey("ImportService"));
        Assert.True(importSettings.ServiceSettings.ContainsKey("Loader"));
        Assert.True(importSettingsProvider.TryGetService<object[]>(importSettings, "LoadStages", out var collection));
        Assert.Equal(2, collection!.Length);
        Assert.False(importSettingsProvider.TryGetService<object>(importSettings, Guid.NewGuid().ToString(), out var _));
    }

    [Fact]
    public void TryGet_DeserializesAllPropertiesIntoDictionary()
    {
        using JsonDocument doc = JsonDocument.Parse(_jsonContent);

        Assert.True(doc.RootElement.TryGetProperty("OzonCategories", out JsonElement sectionElement));
        var jsonObject = JsonObject.Create(sectionElement);
        Assert.NotNull(jsonObject);
        var importSettings = new ImportSettings(jsonObject);
        IImportSettingsProvider importSettingsProvider = new ImportSettingsProvider();

        var result = importSettingsProvider.TryGet<Dictionary<string, JsonElement>>(importSettings, out var value, out var exception);

        Assert.True(result);
        Assert.NotNull(value);
        Assert.Null(exception);
        Assert.True(value.ContainsKey("Name"));
        Assert.True(value.ContainsKey("Caption"));
        Assert.True(value.ContainsKey("Url"));
        Assert.True(value.ContainsKey("CategorySourceUrl"));
        Assert.True(value.ContainsKey("Services"));

        var services = JsonSerializer.Deserialize<Dictionary<string, object>>(value["Services"].ToString());
        Assert.NotEmpty(services);

        var loadStages = JsonSerializer.Deserialize<LoadStageSettings[]>(services["LoadStages"].ToString());
        Assert.NotEmpty(loadStages);
    }

    [Fact]
    public void TryGet_DeserializesIntoCustomPoco()
    {
        using JsonDocument doc = JsonDocument.Parse(_jsonContent);

        Assert.True(doc.RootElement.TryGetProperty("OzonCategories", out JsonElement sectionElement));
        var jsonObject = JsonObject.Create(sectionElement);
        Assert.NotNull(jsonObject);
        var importSettings = new ImportSettings(jsonObject);
        IImportSettingsProvider importSettingsProvider = new ImportSettingsProvider();

        var result = importSettingsProvider.TryGet<OzonCategorySettings>(importSettings, out var value, out var exception);

        Assert.True(result);
        Assert.NotNull(value);
        Assert.Null(exception);
        Assert.Equal("Ozon", value.Name);
        Assert.Equal("Ozon", value.Caption);
        Assert.Equal("https://www.ozon.ru", value.Url);
        Assert.Equal("https://www.ozon.ru", value.CategorySourceUrl);
        Assert.NotEmpty(value.Services);

        var loadStages = JsonSerializer.Deserialize<LoadStageSettings[]>(value.Services["LoadStages"].ToString());
        Assert.NotEmpty(loadStages);
    }
}