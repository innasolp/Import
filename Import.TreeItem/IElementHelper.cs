namespace Import.TreeItem;

public interface IElementHelper<TElement>
{
    bool TryGetByProperty(TElement element, string property, out TElement result);

    string GetRawText(TElement element);

    int GetInt32(TElement element);

    string? GetString(TElement element);

    bool GetBoolean(TElement element);

    bool TryGetElementEnumerable(TElement element, string? property, out IEnumerable<TElement>? result);

    bool IsEmpty(TElement element);
}