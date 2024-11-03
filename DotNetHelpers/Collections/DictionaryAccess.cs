using System.Collections;

namespace DotNetHelpers.Collections;

public class DictionaryAccess<TKey, TValue>(IDictionary<TKey, TValue>? sourceDictionary) 
    : IReadOnlyCollection<(TKey name, TValue value)>
{
    public virtual TValue? this[TKey name]
        => (sourceDictionary is { } dic && dic.TryGetValue(name, out var v)) ? v : default;

    public int Count
        => sourceDictionary?.Count ?? 0;

    IEnumerator<(TKey name, TValue value)> IEnumerable<(TKey name, TValue value)>.GetEnumerator()
        => (sourceDictionary?.Select(kv => (kv.Key, kv.Value)) ?? []).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IReadOnlyCollection<(TKey, TValue)>)this).GetEnumerator();
}