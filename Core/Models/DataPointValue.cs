namespace Core.Models;

/// <summary>报警级别</summary>
public enum AlarmLevel
{
    None,
    Warning,
    Extreme
}

/// <summary>
/// 数据点运行时值 —— 由生产者写入 DataPointStore，订阅者消费。
/// 相当于 MQTT 的一条消息（主题 + 负载 + 时间戳）。
/// </summary>
public class DataPointValue
{
    /// <summary>数据点 Key，格式 "设备Id.点名"（= MQTT 的 Topic）</summary>
    public string PointKey { get; set; } = string.Empty;

    /// <summary>设备 Id</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>数据点名</summary>
    public string PointName { get; set; } = string.Empty;

    /// <summary>原始值</summary>
    public object? RawValue { get; set; }

    /// <summary>换算后的数值（适用数值型）</summary>
    public double? NumericValue { get; set; }

    /// <summary>格式化显示值（含单位）</summary>
    public string FormattedValue { get; set; } = string.Empty;

    /// <summary>单位</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>采集时间</summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>是否处于报警状态（报警引擎回写）</summary>
    public bool IsAlarm { get; set; }

    /// <summary>当前报警级别</summary>
    public AlarmLevel AlarmLevel { get; set; } = AlarmLevel.None;

    /// <summary>质量：Good / Bad</summary>
    public string Quality { get; set; } = "Good";
}
