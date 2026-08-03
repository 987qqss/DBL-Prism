using Core.Models;

namespace Core.Interfaces;

/// <summary>
/// 报警引擎 —— 订阅 DataPointStore 数据，按数据点配置的阈值判定报警。
/// 状态机：Normal → Pending → Warning/Extreme → 恢复。带去抖、迟滞、升级。
/// </summary>
public interface IAlarmEngine : IDataPointSubscriber
{
    /// <summary>开始订阅数据</summary>
    void Start();

    /// <summary>停止订阅</summary>
    void Stop();

    /// <summary>报警触发时触发</summary>
    event Action<AlarmEvent>? AlarmRaised;

    /// <summary>报警恢复时触发</summary>
    event Action<AlarmEvent>? AlarmRecovered;

    /// <summary>当前所有活跃报警</summary>
    IReadOnlyList<AlarmEvent> GetActiveAlarms();

    /// <summary>是否曾触发过报警（测试程序用）</summary>
    bool HasEverAlarmed();

    /// <summary>复位报警记忆</summary>
    void ResetEverAlarmed();

    /// <summary>配置变更时刷新（报警规则变了强制重新评估）</summary>
    void RefreshRules();
}
