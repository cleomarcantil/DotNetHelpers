using System.Collections;
using System.Collections.Concurrent;

namespace DotNetHelpers.Notification;

public class DebounceChangeNotifier<TKey, TValue> : IDisposable
    where TKey : notnull
{
    private readonly int _interval;
    private readonly ChangesRepository _changes;
    private readonly TaskExecutor _taskExecutor;
    private readonly ChangesObservers<IDebounceChangeObserver<TKey, TValue>> _observers;

    public DebounceChangeNotifier(int interval)
    {
        _interval = interval;
        _changes = new();
        _taskExecutor = new(OnMonitoring);
        _observers = new();
    }

    public void Dispose()
        => _taskExecutor.Dispose();

    public void NotifyChanged(TKey key, TValue value)
        => _changes.Add(key, value);

    private async Task OnMonitoring(CancellationToken cancelationToken)
    {
        while (!_observers.IsEmpty && !cancelationToken.IsCancellationRequested)
        {
            await Task.Delay(_interval, cancelationToken);

            if (_changes.Extract() is { Count: > 0 } changesToNotify)
                await _observers.Notify(changesToNotify, cancelationToken);
        }
    }

    public void AddObserver(IDebounceChangeObserver<TKey, TValue> observer)
    {
        _observers.Add(observer);
        _taskExecutor.EnsureStarted();
    }

    public void RemoveObserver(IDebounceChangeObserver<TKey, TValue> observer)
        => _observers.Remove(observer);

    public async Task Watch(ChangedCallback changedCallback, CancellationToken cancellationToken)
    {
        WatchCallbackObserver transientObserver = new(changedCallback);

        AddObserver(transientObserver);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, cancellationToken);
            }
            catch (Exception)
            {
            }
        }

        RemoveObserver(transientObserver);
    }

    #region Internal

    class ChangesRepository
    {
        private readonly ConcurrentDictionary<TKey, TValue> _changes = new();
        private DateTime? _lastChangedTime;
        private object _accessLock = new();

        public void Add(TKey key, TValue value)
        {
            lock (_accessLock)
            {
                _changes.AddOrUpdate(key, (_) => value, (_, _) => value);
                _lastChangedTime = DateTime.Now;
            }
        }

        public ChangesCollected? Extract()
        {
            lock (_accessLock)
            {
                if (_lastChangedTime is null || _changes.IsEmpty)
                    return null;
                
                var changesToNotify = _changes.ToArray();
                _changes.Clear();
                _lastChangedTime = null;

                return new ChangesCollected(changesToNotify);
            }
        }
    }

    class ChangesCollected(KeyValuePair<TKey, TValue>[]? changes)
        : IReadOnlyCollection<(TKey key, TValue value)>
    {
        public int Count
            => (changes?.Length ?? 0);

        public IEnumerator<(TKey key, TValue value)> GetEnumerator()
            => (changes?.Select(c => (c.Key, c.Value)) ?? []).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();
    }

    class TaskExecutor(TaskExecutorAction action) : IDisposable
    {
        private Task? _task = null;
        private CancellationTokenSource? _cancelationTokenSource;
        private readonly object _taskLock = new();

        public void Dispose()
            => Terminate();

        public void EnsureStarted()
        {
            lock (_taskLock)
            {
                if (_task != null)
                    return;

                _cancelationTokenSource = new();
                _task = Task.Run(async () =>
                {
                    await action.Invoke(_cancelationTokenSource.Token);
                    Terminate();
                });
            }
        }

        private void Terminate()
        {
            lock (_taskLock)
            {
                _cancelationTokenSource?.Cancel();
                _cancelationTokenSource?.Dispose();
                _cancelationTokenSource = null;
                _task = null;
            }
        }
    }

    delegate Task TaskExecutorAction(CancellationToken cancellationToken);

    class ChangesObservers<TObserver>
        where TObserver : IDebounceChangeObserver<TKey, TValue>
    {
        private readonly HashSet<TObserver> _observers = new();
        private readonly object _lockObservers = new();

        public void Add(TObserver observer)
        {
            lock (_lockObservers)
            {
                _observers.Add(observer);
            }
        }

        public void Remove(TObserver observer)
        {
            lock (_lockObservers)
            {
                _observers.Remove(observer);
            }
        }

        public bool IsEmpty
        {
            get
            {
                lock (_lockObservers)
                {
                    return (_observers.Count == 0);
                }
            }
        }

        private TObserver[] GetObservers()
        {
            lock (_lockObservers)
            {
                return _observers.ToArray();
            }
        }

        public async Task Notify(ChangesCollected changes, CancellationToken cancellationToken)
        {
            foreach (var observer in GetObservers())
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    await observer.OnChanged(changes, cancellationToken);
                }
                catch (Exception)
                {
                }
            }
        }
    }

    class WatchCallbackObserver(ChangedCallback callback) : IDebounceChangeObserver<TKey, TValue>
    {
        public async Task OnChanged(IReadOnlyCollection<(TKey key, TValue value)> changes, CancellationToken cancellationToken)
            => await callback.Invoke(changes);
    }

    #endregion

    public delegate Task ChangedCallback(IReadOnlyCollection<(TKey key, TValue value)> changes);
}

public interface IDebounceChangeObserver<TKey, TValue>
{
    Task OnChanged(IReadOnlyCollection<(TKey key, TValue value)> changes, CancellationToken cancellationToken);
}