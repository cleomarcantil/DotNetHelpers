using System.Collections;
using System.Collections.Concurrent;

namespace DotNetHelpers.Notification;

public class DebounceChangeNotifier<TKey, TValue>(int interval) : IDisposable
{
    private readonly ConcurrentDictionary<TKey, TValue> _changes = new();
    private readonly ConcurrentDictionary<string, ChangedCallback> _monitors = new();

    private object _changedLock = new();
    private DateTime? _lastChangedTime;

    private readonly object _monitoringTaskLock = new();
    private Task? _monitoringTask;
    private CancellationTokenSource _monitoringTaskToken = new();

    public void Dispose()
    {
        _monitoringTaskToken.Cancel();
        _monitoringTaskToken.Dispose();
    }

    public void NotifyChanged(TKey key, TValue value)
    {
        lock (_changedLock)
        {
            _changes.AddOrUpdate(key, (_) => value, (_, _) => value);
            _lastChangedTime = DateTime.Now;
        }
    }

    private InternalChangesAccess? ExtractChangesToNotify()
    {
        lock (_changedLock)
        {
            if (_lastChangedTime is null)
                return null;
            
            var changesToNotify = _changes.ToArray();
            _changes.Clear();
            _lastChangedTime = null;

            return new InternalChangesAccess(changesToNotify);
        }
    }

    public async Task Monitore(ChangedCallback changedCallback, CancellationToken cancellationToken)
    {
        var monitorKey = Guid.NewGuid().ToString();

        if (_monitors.TryAdd(monitorKey, changedCallback))
        {
            EnsureStartedMonitoringTask();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(interval, cancellationToken);
                }
                catch (Exception)
                {
                }
            }

            _monitors.TryRemove(monitorKey, out var _);
        }
    }

    private void EnsureStartedMonitoringTask()
    {
        lock (_monitoringTaskLock)
        {
            if (_monitoringTask is null)
            {
                _monitoringTask = RunMonitoring(
                    onComplete: () =>
                    {
                        lock (_monitoringTaskLock)
                        {
                            _monitoringTask = null;
                        }
                    });
            }
        }
    }

    private async Task RunMonitoring(Action onComplete)
    {
        while (!_monitors.IsEmpty && !_monitoringTaskToken.IsCancellationRequested)
        {
            await Task.Delay(interval, _monitoringTaskToken.Token);

            if (ExtractChangesToNotify() is { Count: > 0 } changesToNotify)
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

        onComplete.Invoke();
    }

    public delegate Task ChangedCallback(IReadOnlyCollection<(TKey key, TValue value)> changes);

    private class InternalChangesAccess(KeyValuePair<TKey, TValue>[]? changes) 
        : IReadOnlyCollection<(TKey key, TValue value)>
    {
        public int Count 
            => (changes?.Length ?? 0);

        public IEnumerator<(TKey key, TValue value)> GetEnumerator()
            => (changes?.Select(c => (c.Key, c.Value)) ?? []).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();
    }
}