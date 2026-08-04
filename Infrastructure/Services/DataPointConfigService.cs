using System.Text.Json;
using Core;
using Core.Interfaces;
using Core.Models;

namespace Infrastructure.Services;

/// <summary>
/// 数据点配置服务 —— 独立数据点配置的 JSON 持久化。
/// </summary>
public class DataPointConfigService : IDataPointConfigService
{
    private readonly ILogService _logService;
    private readonly string _filePath;
    private readonly List<DataPointConfig> _configs = new();

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public IReadOnlyList<DataPointConfig> Configs => _configs;

    public DataPointConfigService(ILogService logService)
    {
        _logService = logService;
        _filePath = AppPaths.DataPointsFile;
        AppPaths.EnsureDirectories();
    }

    public void Load()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                LoadDefaults();
                Save();
                return;
            }

            var json = File.ReadAllText(_filePath);
            var list = JsonSerializer.Deserialize<List<DataPointConfig>>(json, JsonOpts);
            _configs.Clear();
            if (list != null)
                _configs.AddRange(list);

            _logService.Info($"数据点配置加载完成 ({_configs.Count} 个)", "DataPoint");
        }
        catch (Exception ex)
        {
            _logService.Error($"加载数据点配置失败: {ex.Message}", "DataPoint");
        }
    }

    public void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(_configs, JsonOpts);
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            _logService.Error($"保存数据点配置失败: {ex.Message}", "DataPoint");
        }
    }

    public void AddOrUpdate(DataPointConfig config)
    {
        if (config == null) return;

        var idx = _configs.FindIndex(c => c.Id == config.Id);
        if (idx >= 0)
            _configs[idx] = config;
        else
            _configs.Add(config);

        Save();
    }

    public void Remove(string configId)
    {
        _configs.RemoveAll(c => c.Id == configId);
        Save();
    }

    /// <summary>
    /// 根据设备命令的监测标记增量同步配置（勾选即更新，无需全量扫描）。
    /// IsMonitored 或 IsAlarmEnabled 为 true → 创建/更新对应 DataPointConfig；
    /// 两者都为 false → 删除已有配置。
    /// </summary>
    public void SyncFromCommand(DeviceModel device, DeviceCommand command)
    {
        if (device == null || command == null) return;

        // 定位：同一设备 + 同一命令名 = 同一数据点
        var existing = _configs.FirstOrDefault(c =>
            c.DeviceId == device.Id && c.PointName == command.Name);

        bool shouldMonitor = command.IsMonitored || command.IsAlarmEnabled;

        if (!shouldMonitor)
        {
            // 全部取消：删除已有配置
            if (existing != null)
            {
                _configs.Remove(existing);
                _logService.Info($"监测配置已移除: {device.Name}.{command.Name}", "DataPoint");
                Save();
            }
            return;
        }

        // 已标记：创建或更新配置
        if (existing == null)
        {
            existing = new DataPointConfig
            {
                Id = Guid.NewGuid().ToString("N"),
                DeviceId = device.Id,
                PointName = command.Name,
                Enabled = true,
                Source = "Poll",
                PollIntervalMs = 1000,
                ProtocolAddress = command.ProtocolAddress,
                DataFormat = command.DataFormat,
                Scale = command.Scale,
                Offset = command.Offset,
                Unit = command.Unit
            };
            _configs.Add(existing);
        }
        else
        {
            // 更新命令可能变化的地址/格式/单位
            existing.ProtocolAddress = command.ProtocolAddress;
            existing.DataFormat = command.DataFormat;
            existing.Scale = command.Scale;
            existing.Offset = command.Offset;
            existing.Unit = command.Unit;
            existing.Enabled = true;
        }

        // 报警标记
        existing.EnableAlarm = command.IsAlarmEnabled;
        existing.UpperLimit = command.AlarmUpperLimit;
        existing.LowerLimit = command.AlarmLowerLimit;

        _logService.Info(
            $"监测配置已同步: {device.Name}.{command.Name} (报警:{(command.IsAlarmEnabled ? "开" : "关")})",
            "DataPoint");
        Save();
    }

    public IEnumerable<DataPointConfig> GetByDevice(string deviceId)
        => _configs.Where(c => c.DeviceId == deviceId);

    /// <summary>加载默认示例数据（首次启动）</summary>
    private void LoadDefaults()
    {
        _configs.Add(new DataPointConfig
        {
            DeviceId = "DEV001",
            PointName = "总电压",
            ProtocolAddress = "03:1000:2",
            DataFormat = DataFormat.Int16,
            Scale = 0.1f,
            Unit = "V",
            PollIntervalMs = 1000,
            Source = "Poll",
            EnableAlarm = true,
            UpperLimit = 480,
            UpperExtreme = 500,
            DebounceMs = 1000
        });

        _configs.Add(new DataPointConfig
        {
            DeviceId = "DEV001",
            PointName = "电芯温度",
            ProtocolAddress = "04:1010:4",
            DataFormat = DataFormat.Int16,
            Unit = "℃",
            PollIntervalMs = 2000,
            Source = "Poll",
            EnableAlarm = true,
            UpperLimit = 55,
            UpperExtreme = 65,
            DebounceMs = 1000
        });

        _configs.Add(new DataPointConfig
        {
            DeviceId = "DEV002",
            PointName = "有功功率",
            ProtocolAddress = "03:1100:2",
            DataFormat = DataFormat.Int16,
            Scale = 0.01f,
            Unit = "kW",
            PollIntervalMs = 1000,
            Source = "Poll"
        });
    }
}
