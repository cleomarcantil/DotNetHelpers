using System.Collections;

namespace DotNetHelpers.Collections;

public class OrderedQueue<T> : IReadOnlyCollection<T>
    where T : IComparable<T>
{
    private readonly SortedList<KeyItem, object?> _values = new();
    private long _sequence = 0;
    private readonly object? _lockValues = null;


    public OrderedQueue(IEnumerable<T> values, bool concurrencyLock = false)
    {
        if (concurrencyLock)
            _lockValues = new();

        AddRange(values);
    }

    public OrderedQueue(bool concurrencyLock = false)
        : this([], concurrencyLock)
    {
    }

    public void Add(T value)
        => AddRange(value);

    public void AddRange(params IEnumerable<T> values)
        => OnLockContext(() =>
        {
            foreach (var v in values)
            {
                _values.Add(new(v, ++_sequence), null);
            }
        });

    public bool TryDequeue(out T result)
        => TryDequeueIf(x => true, out result);

    public bool TryDequeueIf(Func<T, bool> predicade, out T value)
    {
        (var dequeued, value) = OnLockContext(() =>
        {
            var key = (_values.Count > 0) ? _values.GetKeyAtIndex(0) : null;

            if (key is not null && predicade(key.Value) && _values.Remove(key))
                return (true, key.Value);
            else
                return (false, default!);
        });

        return dequeued;
    }


    #region IReadOnlyCollection

    public int Count
        => _values.Count;

    public IEnumerator<T> GetEnumerator()
        => _values.Keys.Select(x => x.Value).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => this.GetEnumerator();

    #endregion

    #region Internal

    private R OnLockContext<R>(Func<R> action)
    {
        if (_lockValues != null)
            lock (_lockValues) return action.Invoke();
        else
            return action.Invoke();
    }

    private void OnLockContext(Action action)
        => OnLockContext<object?>(() => { action.Invoke(); return null; });

    record KeyItem(T Value, long Sequence) : IComparable<KeyItem>
    {
        public int CompareTo(KeyItem? other)
            => (Value.CompareTo(other!.Value)) switch
            {
                0 => Sequence.CompareTo(other.Sequence),
                var c => c,
            };
    }

    #endregion
}