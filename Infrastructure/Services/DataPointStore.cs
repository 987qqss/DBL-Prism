using System.Collections.Concurrent;
using Core.Interfaces;
using Core.Models;

namespace Infrastructure.Services;

/// <summary>
/// 数据点存储（数据中心 / Broker）——
/// 生产者（采集服务）调用 Publish 写入，订阅者（报警/监控/报表）通过 Subscribe 接收。
/// 维护最新值缓存 + 数据点定义列表。
/// </summary>
public class DataPointStore : IDataPointStore
{
    private readonly ConcurrentDictionary<string, DataPointValue> _latest = new();
    private readonly ConcurrentDictionary<string, DataPointConfig> _definitions = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, DataPointValue> _byDevice = new();

    // 订阅者列表（广播式）
    private readonly List<IDataPointSubscriber> _subscribers = new();
    private readonly object _subLock = new();

    public event Action? DefinitionsChanged;

    public IReadOnlyList<DataPointConfig> Definitions => _definitions.Values.ToList();

    // ─── 定义管理 ───

    public void RegisterDefinition(DataPointConfig config)
    {
        if (config == null || string.IsNullOrWhiteSpace(config.PointName)) return;
        _definitions[config.Id] = config;
        DefinitionsChanged?.Invoke();
    }

    public void RemoveDefinition(string configId)
    {
        if (_definitions.TryRemove(configId, out var def))
        {
            // 同时清理该点最新值
            var key = BuildKey(def.DeviceId, def.PointName);
            _latest.TryRemove(key, out _);
            _byDevice.TryRemove(key, out _);
            DefinitionsChanged?.Invoke();
        }
    }

    // ─── 发布 / 订阅 ───

    public void Publish(DataPointValue value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.PointKey)) return;

        _latest[value.PointKey] = value;
        _byDevice[value.PointKey] = value;

        // 广播给所有订阅者
        List<IDataPointSubscriber> snapshot;
        lock (_subLock)
        {
            snapshot = _subscribers.ToList();
        }

        foreach (var sub in snapshot)
        {
            try { sub.OnDataPointUpdated(value); }
            catch { /* 单个订阅者异常不影响其他订阅者 */ }
        }
    }

    public void Subscribe(IDataPointSubscriber subscriber)
    {
        if (subscriber == null) return;
        lock (_subLock)
        {
            if (!_subscribers.Contains(subscriber))
                _subscribers.Add(subscriber);
        }
    }

    public void Unsubscribe(IDataPointSubscriber subscriber)
    {
        lock (_subLock)
        {
            _subscribers.Remove(subscriber);
        }
    }

    // ─── 查询 ───

    public DataPointValue? GetLatest(string pointKey)
        => _latest.TryGetValue(pointKey, out var v) ? v : null;

    public IReadOnlyDictionary<string, DataPointValue> GetDeviceSnapshot(string deviceId)
        => _byDevice
            .Where(kvp => kvp.Key.StartsWith(deviceId + ".", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    public IReadOnlyDictionary<string, DataPointValue> GetAllLatest()
        => new Dictionary<string, DataPointValue>(_latest);

    /// <summary>生成数据点 Key：设备Id.点名</summary>
    public static string BuildKey(string deviceId, string pointName)
        => $"{deviceId}.{pointName}";
}
