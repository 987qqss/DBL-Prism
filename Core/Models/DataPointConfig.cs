namespace Core.Models;

/// <summary>
/// 数据点配置 —— 独立于设备命令的监测点定义。
/// 一份配置决定一个"主题"：从哪采集、采集周期、报警阈值。
/// 用户可独立配置（JSON 持久化），系统围绕这份配置运转。
/// </summary>
public class DataPointConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>所属设备 Id</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>数据点名（= 主题 Key 的一部分，如 "总电压"）</summary>
    public string PointName { get; set; } = string.Empty;

    /// <summary>协议地址（如 "03:1000:2"），从设备读取时使用</summary>
    public string ProtocolAddress { get; set; } = string.Empty;

    /// <summary>数据格式（解析原始值）</summary>
    public DataFormat DataFormat { get; set; } = DataFormat.Int16;

    /// <summary>转换系数：显示值 = 原始值 × Scale + Offset</summary>
    public float Scale { get; set; } = 1.0f;

    /// <summary>偏移量</summary>
    public float Offset { get; set; } = 0.0f;

    /// <summary>单位（如 V、℃、%）</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>采集周期（毫秒），Source=Poll 时使用</summary>
    public int PollIntervalMs { get; set; } = 1000;

    /// <summary>数据来源：Poll（轮询设备） / Mqtt（订阅消息）</summary>
    public string Source { get; set; } = "Poll";

    /// <summary>Source=Mqtt 时的订阅主题</summary>
    public string? MqttTopic { get; set; }

    /// <summary>是否启用此数据点</summary>
    public bool Enabled { get; set; } = true;

    // ─── 报警配置 ───

    /// <summary>是否启用报警</summary>
    public bool EnableAlarm { get; set; }

    /// <summary>报警上限</summary>
    public double? UpperLimit { get; set; }

    /// <summary>报警下限</summary>
    public double? LowerLimit { get; set; }

    /// <summary>上极限（越限立即升级为紧急报警）</summary>
    public double? UpperExtreme { get; set; }

    /// <summary>下极限</summary>
    public double? LowerExtreme { get; set; }

    /// <summary>去抖时间（毫秒）：值持续超限多久才触发报警</summary>
    public int DebounceMs { get; set; } = 500;

    /// <summary>迟滞百分比：恢复时需回到阈值内多少才解除报警</summary>
    public double HysteresisPercent { get; set; } = 5.0;

    /// <summary>备注</summary>
    public string Remark { get; set; } = string.Empty;
}
