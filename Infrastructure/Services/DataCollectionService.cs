using Core.Interfaces;
using Core.Models;

namespace Infrastructure.Services;

/// <summary>
/// 数据采集服务（生产者）——
/// 读取数据点配置，对 Source=Poll 的数据点按周期轮询设备，
/// 对 Source=Mqtt 的数据点订阅消息主题，结果统一写入 DataPointStore。
/// </summary>
public class DataCollectionService : IDisposable
{
    private readonly IDataPointStore _store;
    private readonly IDataPointConfigService _configService;
    private readonly IDeviceExecutionService _executionService;
    private readonly IMqttService _mqttService;
    private readonly ILogService _logService;
    private readonly IConfigurationService _config;

    private CancellationTokenSource? _cts;
    private readonly List<Task> _pollTasks = new();
    private readonly object _taskLock = new();
    private bool _disposed;

    public bool IsRunning => _cts != null && !_cts.IsCancellationRequested;

    public DataCollectionService(
        IDataPointStore store,
        IDataPointConfigService configService,
        IDeviceExecutionService executionService,
        IMqttService mqttService,
        ILogService logService,
        IConfigurationService config)
    {
        _store = store;
        _configService = configService;
        _executionService = executionService;
        _mqttService = mqttService;
        _logService = logService;
        _config = config;
    }

    /// <summary>启动采集：为每个启用的轮询数据点启动后台采集任务，订阅 MQTT 数据点</summary>
    public void Start()
    {
        if (IsRunning) return;

        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        // 把配置同步到 DataPointStore（注册定义）
        foreach (var cfg in _configService.Configs)
            if (cfg.Enabled)
                _store.RegisterDefinition(cfg);

        lock (_taskLock)
        {
            _pollTasks.Clear();

            foreach (var cfg in _configService.Configs.Where(c => c.Enabled))
            {
                if (string.Equals(cfg.Source, "Mqtt", StringComparison.OrdinalIgnoreCase))
                {
                    // MQTT 数据点：订阅主题
                    SubscribeMqtt(cfg, ct);
                }
                else
                {
                    // 轮询数据点：启动后台采集任务
                    var task = Task.Run(() => PollLoopAsync(cfg, ct), ct);
                    _pollTasks.Add(task);
                }
            }
        }

        _logService.Info($"数据采集服务已启动 ({_configService.Configs.Count(c => c.Enabled)} 个数据点)", "DataCollection");
    }

    /// <summary>停止采集</summary>
    public void Stop()
    {
        _cts?.Cancel();
        try { Task.WaitAll(_pollTasks.ToArray(), TimeSpan.FromSeconds(2)); }
        catch { /* 忽略停止超时 */ }

        _cts?.Dispose();
        _cts = null;
        _logService.Info("数据采集服务已停止", "DataCollection");
    }

    /// <summary>单次轮询一个数据点（供手动触发/测试）</summary>
    public async Task<bool> PollOnceAsync(DataPointConfig cfg)
    {
        try
        {
            var value = await ReadDevicePointAsync(cfg);
            if (value != null)
            {
                _store.Publish(value);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logService.Debug($"轮询数据点失败: {cfg.DeviceId}.{cfg.PointName}: {ex.Message}", "DataCollection");
            return false;
        }
    }

    // ─── 轮询循环 ───

    private async Task PollLoopAsync(DataPointConfig cfg, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var value = await ReadDevicePointAsync(cfg);
                if (value != null)
                    _store.Publish(value);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logService.Debug($"轮询数据点异常: {cfg.DeviceId}.{cfg.PointName}: {ex.Message}", "DataCollection");
            }

            try { await Task.Delay(cfg.PollIntervalMs, ct); }
            catch (OperationCanceledException) { break; }
        }
    }

    /// <summary>从设备读取一个数据点并转换为 DataPointValue</summary>
    private async Task<DataPointValue?> ReadDevicePointAsync(DataPointConfig cfg)
    {
        // 确保设备已连接
        var device = FindDevice(cfg.DeviceId);
        if (device == null)
            return null;

        if (!_executionService.IsConnected(cfg.DeviceId))
        {
            var ok = await _executionService.ConnectAsync(device);
            if (!ok) return null;
        }

        var cmd = new DeviceCommand
        {
            Name = cfg.PointName,
            ProtocolAddress = cfg.ProtocolAddress,
            DataFormat = cfg.DataFormat,
            Scale = cfg.Scale,
            Offset = cfg.Offset,
            Unit = cfg.Unit
        };

        var result = await _executionService.ReadAsync(device, cmd);

        return new DataPointValue
        {
            PointKey = DataPointStore.BuildKey(cfg.DeviceId, cfg.PointName),
            DeviceId = cfg.DeviceId,
            PointName = cfg.PointName,
            RawValue = result.RawValue,
            NumericValue = result.ConvertedValue,
            FormattedValue = result.FormattedValue,
            Unit = cfg.Unit,
            Timestamp = DateTime.Now,
            Quality = result.Success ? "Good" : "Bad"
        };
    }

    /// <summary>订阅 MQTT 数据点：收到消息时写入 DataPointStore</summary>
    private void SubscribeMqtt(DataPointConfig cfg, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cfg.MqttTopic)) return;

        var pointKey = DataPointStore.BuildKey(cfg.DeviceId, cfg.PointName);
        string topic = cfg.MqttTopic;

        Action<string, string> handler = (t, payload) =>
        {
            if (!string.Equals(t, topic, StringComparison.OrdinalIgnoreCase)) return;

            var value = new DataPointValue
            {
                PointKey = pointKey,
                DeviceId = cfg.DeviceId,
                PointName = cfg.PointName,
                RawValue = payload,
                NumericValue = double.TryParse(payload, out var n) ? n : null,
                FormattedValue = payload,
                Unit = cfg.Unit,
                Timestamp = DateTime.Now,
                Quality = "Good"
            };
            _store.Publish(value);
        };

        _mqttService.MessageReceived += handler;
        _ = _mqttService.SubscribeAsync(topic, 0);
        _logService.Info($"订阅 MQTT 数据点: {topic} → {pointKey}", "DataCollection");

        // 停止时取消订阅
        ct.Register(() =>
        {
            _mqttService.MessageReceived -= handler;
            _ = _mqttService.UnsubscribeAsync(topic);
        });
    }

    private DeviceModel? FindDevice(string deviceId)
    {
        foreach (var line in _config.ProductionLines)
            foreach (var dev in line.Devices)
                if (dev.Id == deviceId)
                    return dev;
        return null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        Stop();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
