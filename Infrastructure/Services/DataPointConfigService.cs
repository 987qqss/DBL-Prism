using System.Text.Json;
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
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "datapoints.json");
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
