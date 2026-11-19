using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Import.Settings.Json;

public sealed class FixedSizeDictionary<TKey, TValue>
    : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDictionary
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _inner;

    public FixedSizeDictionary(
        IEnumerable<KeyValuePair<TKey, TValue>> source,
        IEqualityComparer<TKey>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        _inner = new Dictionary<TKey, TValue>(source, comparer);
    }

    public bool IsFixedSize => true;
    public bool IsReadOnly => false;
    public int Count => _inner.Count;

    public TValue this[TKey key]
    {
        get => _inner[key];
        set
        {
            if (!_inner.ContainsKey(key))
                throw new NotSupportedException(
                    $"Collection was of a fixed size: cannot add key '{key}'.");

            _inner[key] = value;
        }
    }

    public ICollection<TKey> Keys => _inner.Keys;
    public ICollection<TValue> Values => _inner.Values;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _inner.Keys;
    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _inner.Values;

    public bool ContainsKey(TKey key) => _inner.ContainsKey(key);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        => _inner.TryGetValue(key, out value);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        => _inner.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();

    // Operations that modify the key set are not supported
    public void Add(TKey key, TValue value) => ThrowFixedSize();
    public bool Remove(TKey key) => ThrowFixedSize<bool>();
    public void Clear() => ThrowFixedSize();

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        => ThrowFixedSize();

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        => ThrowFixedSize<bool>();

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        => ((ICollection<KeyValuePair<TKey, TValue>>)_inner).Contains(item);

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(
        KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        => ((ICollection<KeyValuePair<TKey, TValue>>)_inner).CopyTo(array, arrayIndex);

    // Non-generic IDictionary
    ICollection IDictionary.Keys => _inner.Keys;
    ICollection IDictionary.Values => _inner.Values;
    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => ((ICollection)_inner).SyncRoot;

    object? IDictionary.this[object key]
    {
        get => ((IDictionary)_inner)[key];
        set
        {
            ArgumentNullException.ThrowIfNull(key);

            if (key is not TKey typedKey)
                throw new ArgumentException(
                    $"Expected key of type '{typeof(TKey).Name}'.", nameof(key));

            if (!_inner.ContainsKey(typedKey))
                throw new NotSupportedException(
                    $"Collection was of a fixed size: cannot add key '{typedKey}'.");

            if (value is null)
            {
                if (default(TValue) is not null)
                    throw new ArgumentNullException(
                        nameof(value), $"Value of type '{typeof(TValue).Name}' cannot be null.");

                _inner[typedKey] = default!;
                return;
            }

            if (value is not TValue typedValue)
                throw new ArgumentException(
                    $"Expected value of type '{typeof(TValue).Name}'.", nameof(value));

            _inner[typedKey] = typedValue;
        }
    }

    void IDictionary.Add(object key, object? value) => ThrowFixedSize();
    void IDictionary.Remove(object key) => ThrowFixedSize();
    bool IDictionary.Contains(object key) => ((IDictionary)_inner).Contains(key);
    IDictionaryEnumerator IDictionary.GetEnumerator() => ((IDictionary)_inner).GetEnumerator();
    void ICollection.CopyTo(Array array, int index) => ((ICollection)_inner).CopyTo(array, index);

    [DoesNotReturn]
    private static void ThrowFixedSize()
        => throw new NotSupportedException("Collection was of a fixed size.");

    [DoesNotReturn]
    private static T ThrowFixedSize<T>()
        => throw new NotSupportedException("Collection was of a fixed size.");
}