using System.Text.Json.Serialization;

namespace Import.Catalog.Product.Service;

public class CategoryProcessState
{
    [JsonInclude]
    public int SuccessProductCount { get; private set; }

    [JsonInclude]
    public int UnsuccessProductCount { get; private set; }

    [JsonInclude]
    public int AttemptsCount { get; private set; }

    [JsonInclude]
    public int Page { get; private set; } = 1;

    public int AllProductCount => SuccessProductCount + UnsuccessProductCount;

    [JsonInclude]
    public string? CategoryPagePath { get; set; }

    [JsonInclude]
    public string? CategoryPath { get; private set; }

    [JsonConstructor]
    private CategoryProcessState(int successProductCount, int unsuccessProductCount, int attemptsCount, int page)
    {
        SuccessProductCount = successProductCount;
        UnsuccessProductCount = unsuccessProductCount;
        AttemptsCount = attemptsCount;
        Page = page;
    }

    public static CategoryProcessState Start()
    {
        return new CategoryProcessState(0, 0, 0, 1);
    }

    public static CategoryProcessState Start(string categoryPath)
    {
        return new CategoryProcessState(0, 0, 0, 1) { CategoryPath = categoryPath };
    }

    public void Reset()
    {
        SuccessProductCount = 0;
        UnsuccessProductCount = 0;
        AttemptsCount = 0;
        Page = 0;
        CategoryPagePath = null;
    }

    public void ResetAttempts()
    {
        AttemptsCount = 0;
    }

    public void IncrementAttempts()
    {
        AttemptsCount++;
    }

    public void ApplyPageIterationResult(int? iterationSuccessCount, int? iterationFullCount)
    {
        SuccessProductCount += iterationSuccessCount ?? 0;
        UnsuccessProductCount += iterationSuccessCount != null && iterationFullCount != null
            ? iterationFullCount.Value - iterationSuccessCount.Value
            : 0;
    }

    public void IncrementPage()
    {
        Page++;
    }
}