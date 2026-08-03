namespace Core.Models;

/// <summary>报警事件 —— 报警触发/恢复时由报警引擎发布</summary>
public class AlarmEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>数据点 Key</summary>
    public string PointKey { get; set; } = string.Empty;

    /// <summary>设备 Id</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>数据点名</summary>
    public string PointName { get; set; } = string.Empty;

    /// <summary>当前值</summary>
    public double Value { get; set; }

    /// <summary>报警级别</summary>
    public AlarmLevel Level { get; set; }

    /// <summary>报警阈值（触发时的限值）</summary>
    public double Threshold { get; set; }

    /// <summary>触发时间</summary>
    public DateTime TriggerTime { get; set; } = DateTime.Now;

    /// <summary>是否已恢复</summary>
    public bool IsRecovered { get; set; }

    /// <summary>恢复时间（IsRecovered 时有效）</summary>
    public DateTime? RecoverTime { get; set; }

    /// <summary>报警描述文本</summary>
    public string Message { get; set; } = string.Empty;
}
