using System.Collections;

namespace DotNetHelpers.DynamicData;

public abstract class DynamicValuesCollectionBase<T>(bool keepNulls)
    : IReadOnlyCollection<(string name, T value)>
{
    protected internal abstract IDictionary<string, T>? GetSourceDictionary();

    public virtual T? this[string name]
    {
        get => (GetSourceDictionary() is { } dic && dic.TryGetValue(name, out var v)) ? v: default;
        protected set
        {
            var sourceDic = GetSourceDictionary()
                ?? throw new Exception($"SourceDictionary não definido!");

            if (value is null && !keepNulls)
            {
                sourceDic.Remove(name);
                return;
            }

            sourceDic[name] = value;
        }
    }

    public int Count
        => GetSourceDictionary()?.Count ?? 0;

    protected void Clear()
        => GetSourceDictionary()?.Clear();

    public IEnumerator<(string name, T value)> GetEnumerator()
        => (GetSourceDictionary()?.Select(kv => (kv.Key, kv.Value)) ?? []).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => this.GetEnumerator();
}