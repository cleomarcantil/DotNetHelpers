using System.Collections;
using System.Collections.Concurrent;
using System.Threading;

namespace DotNetHelpers.Notification;

public class DebounceChangeNotifier<TKey, TValue> : IDisposable
    where TKey : notnull
{
    private readonly int _interval;
    private readonly ChangesDictionary _changes;
    private readonly TaskExecutor _taskExecutor;

    private readonly ConcurrentDictionary<string, ChangedCallback> _monitors;

    public DebounceChangeNotifier(int interval)
    {
        _interval = interval;
        _changes = new();
        _taskExecutor = new(RunMonitoring);
        _monitors = new();
    }

    public void Dispose()
    {
        _taskExecutor.Dispose();
    }

    public void NotifyChanged(TKey key, TValue value)
        => _changes.Add(key, value);

    public async Task Monitore(ChangedCallback changedCallback, CancellationToken cancellationToken)
    {
        var monitorKey = Guid.NewGuid().ToString();

        if (_monitors.TryAdd(monitorKey, changedCallback))
        {
            _taskExecutor.EnsureStarted();

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

            _monitors.TryRemove(monitorKey, out var _);
        }
    }

    private async Task RunMonitoring(CancellationToken cts)
    {
        while (!_monitors.IsEmpty && !cts.IsCancellationRequested)
        {
            await Task.Delay(_interval, cts);

            if (_changes.Extract() is { Count: > 0 } changesToNotify)
            {
                foreach (var (_, callback) in _monitors)
                {
                    try
                    {
                        await callback.Invoke(changesToNotify);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }

    public delegate Task ChangedCallback(IReadOnlyCollection<(TKey key, TValue value)> changes);


    #region Internal

    class ChangesDictionary
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

        public ChangesAccessor? Extract()
        {
            lock (_accessLock)
            {
                if (_lastChangedTime is null)
                    return null;

                var changesToNotify = _changes.ToArray();
                _changes.Clear();
                _lastChangedTime = null;

                return new ChangesAccessor(changesToNotify);
            }
        }
    }

    class ChangesAccessor(KeyValuePair<TKey, TValue>[]? changes)
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
        {
            _cancelationTokenSource?.Cancel();
            _cancelationTokenSource?.Dispose();
        }

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
                    Stop();
                });
            }
        }

        public void Stop()
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

    #endregion
}