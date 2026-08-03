using System.Collections.Concurrent;
using Core.Interfaces;
using Core.Models;

namespace Infrastructure.Services;

/// <summary>
/// 报警引擎 —— 状态机实现。
/// 每个启用报警的数据点维护一个 SignalAlarmState：
/// Normal → PendingWarning/PendingExtreme → Warning/Extreme → 恢复回 Normal。
/// 去抖：值超限持续 DebounceMs 才真正报警；迟滞：恢复需回到阈值内 HysteresisPercent。
/// </summary>
public class AlarmEngine : IAlarmEngine
{
    private readonly IDataPointStore _store;
    private readonly IDataPointConfigService _configService;
    private readonly ILogService _logService;

    // 数据点Key → 报警状态机
    private readonly ConcurrentDictionary<string, SignalAlarmState> _states = new();

    // 活跃报警
    private readonly ConcurrentDictionary<string, AlarmEvent> _activeAlarms = new();

    // 全程报警记忆
    private bool _everAlarmed;

    public event Action<AlarmEvent>? AlarmRaised;
    public event Action<AlarmEvent>? AlarmRecovered;

    public AlarmEngine(IDataPointStore store, IDataPointConfigService configService, ILogService logService)
    {
        _store = store;
        _configService = configService;
        _logService = logService;
    }

    /// <summary>开始订阅数据</summary>
    public void Start()
    {
        _store.Subscribe(this);
    }

    /// <summary>停止订阅</summary>
    public void Stop()
    {
        _store.Unsubscribe(this);
    }

    // ─── 订阅回调（工作线程） ───

    public void OnDataPointUpdated(DataPointValue value)
    {
        // 查该数据点的报警配置
        var cfg = FindConfig(value.DeviceId, value.PointName);
        if (cfg == null || !cfg.EnableAlarm) return;

        double numeric = value.NumericValue ?? 0;
        EvaluateAlarm(cfg, value.PointKey, value.DeviceId, value.PointName, numeric);
    }

    // ─── 状态机 ───

    private void EvaluateAlarm(DataPointConfig cfg, string key, string deviceId, string pointName, double value)
    {
        var state = _states.GetOrAdd(key, _ => new SignalAlarmState());
        var now = DateTime.Now;

        // 阈值判定（含迟滞）
        bool inWarning = IsWithinLimit(value, cfg, cfg.UpperLimit, cfg.LowerLimit, cfg.HysteresisPercent, out _);
        bool inExtreme = IsWithinLimit(value, cfg, cfg.UpperExtreme, cfg.LowerExtreme, cfg.HysteresisPercent, out _);

        switch (state.Phase)
        {
            case AlarmPhase.Normal:
                if (!inExtreme && !inWarning)
                {
                    // 超限
                    state.EnterPending(cfg, inExtreme, value, now);
                }
                break;

            case AlarmPhase.PendingWarning:
                if (inWarning)
                {
                    state.Phase = AlarmPhase.Normal; // 未到去抖时间就恢复
                }
                else if (!inExtreme && (now - state.FirstTriggerTime).TotalMilliseconds >= cfg.DebounceMs)
                {
                    ActivateAlarm(state, key, deviceId, pointName, value, AlarmLevel.Warning, cfg.UpperLimit ?? cfg.LowerLimit ?? 0);
                }
                else if (inExtreme)
                {
                    state.Phase = AlarmPhase.PendingExtreme;
                }
                break;

            case AlarmPhase.PendingExtreme:
                if (inExtreme)
                {
                    state.Phase = AlarmPhase.Normal;
                }
                else if ((now - state.FirstTriggerTime).TotalMilliseconds >= cfg.DebounceMs)
                {
                    ActivateAlarm(state, key, deviceId, pointName, value, AlarmLevel.Extreme, cfg.UpperExtreme ?? cfg.LowerExtreme ?? 0);
                }
                break;

            case AlarmPhase.Warning:
                if (inWarning)
                {
                    RecoverAlarm(state, key);
                }
                else if (!inExtreme && !inWarning)
                {
                    // 进入极值
                    state.Phase = AlarmPhase.PendingExtreme;
                    state.FirstTriggerTime = now;
                }
                break;

            case AlarmPhase.Extreme:
                if (inExtreme)
                {
                    RecoverAlarm(state, key);
                }
                break;
        }
    }

    private void ActivateAlarm(SignalAlarmState state, string key, string deviceId, string pointName,
        double value, AlarmLevel level, double threshold)
    {
        state.Phase = level == AlarmLevel.Extreme ? AlarmPhase.Extreme : AlarmPhase.Warning;

        var evt = new AlarmEvent
        {
            PointKey = key,
            DeviceId = deviceId,
            PointName = pointName,
            Value = value,
            Level = level,
            Threshold = threshold,
            TriggerTime = DateTime.Now,
            Message = $"{pointName} 越限: {value} (阈值 {threshold})"
        };

        _activeAlarms[key] = evt;
        _everAlarmed = true;
        _logService.Error($"⚠ 报警触发 [{level}] {pointName}: {value} (阈值 {threshold})", "Alarm");
        AlarmRaised?.Invoke(evt);
    }

    private void RecoverAlarm(SignalAlarmState state, string key)
    {
        if (_activeAlarms.TryRemove(key, out var evt))
        {
            evt.IsRecovered = true;
            evt.RecoverTime = DateTime.Now;
            state.Phase = AlarmPhase.Normal;
            _logService.Info($"报警恢复: {evt.PointName}", "Alarm");
            AlarmRecovered?.Invoke(evt);
        }
        else
        {
            state.Phase = AlarmPhase.Normal;
        }
    }

    /// <summary>判断值是否在限值范围内（含迟滞）</summary>
    private static bool IsWithinLimit(double value, DataPointConfig cfg,
        double? upper, double? lower, double hysteresisPercent, out double violatedThreshold)
    {
        violatedThreshold = 0;

        if (upper.HasValue)
        {
            var hysteresis = Math.Abs(upper.Value * hysteresisPercent / 100.0);
            if (value > upper.Value + hysteresis) { violatedThreshold = upper.Value; return false; }
        }
        if (lower.HasValue)
        {
            var hysteresis = Math.Abs(lower.Value * hysteresisPercent / 100.0);
            if (value < lower.Value - hysteresis) { violatedThreshold = lower.Value; return false; }
        }
        return true;
    }

    // ─── 查询 ───

    public IReadOnlyList<AlarmEvent> GetActiveAlarms() => _activeAlarms.Values.ToList();
    public bool HasEverAlarmed() => _everAlarmed;
    public void ResetEverAlarmed() => _everAlarmed = false;

    public void RefreshRules()
    {
        // 配置变更：移除已不存在的规则对应的活跃报警
        var validKeys = _configService.Configs
            .Where(c => c.EnableAlarm)
            .Select(c => DataPointStore.BuildKey(c.DeviceId, c.PointName))
            .ToHashSet();

        var toRemove = _activeAlarms.Keys.Where(k => !validKeys.Contains(k)).ToList();
        foreach (var key in toRemove)
            RecoverAlarm(_states.GetOrAdd(key, _ => new SignalAlarmState()), key);
    }

    private DataPointConfig? FindConfig(string deviceId, string pointName)
    {
        return _configService.Configs.FirstOrDefault(c =>
            c.DeviceId == deviceId && c.PointName == pointName);
    }

    // ─── 内部状态 ───

    private enum AlarmPhase { Normal, PendingWarning, PendingExtreme, Warning, Extreme }

    private class SignalAlarmState
    {
        public AlarmPhase Phase { get; set; } = AlarmPhase.Normal;
        public DateTime FirstTriggerTime { get; set; }

        public void EnterPending(DataPointConfig cfg, bool extreme, double value, DateTime now)
        {
            FirstTriggerTime = now;
            Phase = extreme ? AlarmPhase.PendingExtreme : AlarmPhase.PendingWarning;
        }
    }
}
