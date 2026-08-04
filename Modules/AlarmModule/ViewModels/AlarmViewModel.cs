using System.Collections.ObjectModel;
using System.Windows.Threading;
using Core.Interfaces;
using Core.Models;
using Prism.Mvvm;

namespace AlarmModule.ViewModels;

/// <summary>
/// 报警 ViewModel —— 订阅报警引擎事件，维护报警列表。
/// 用于报警独立窗口（AlarmWindow）。
/// </summary>
public class AlarmViewModel : BindableBase
{
    private readonly IAlarmEngine _alarmEngine;
    private readonly ILogService _logService;
    private readonly Dispatcher _dispatcher;

    private string _statusText = "暂无报警";
    private int _activeCount;

    public AlarmViewModel(IAlarmEngine alarmEngine, ILogService logService)
    {
        _alarmEngine = alarmEngine;
        _logService = logService;
        _dispatcher = System.Windows.Application.Current?.Dispatcher
            ?? Dispatcher.CurrentDispatcher;

        // 订阅报警事件
        _alarmEngine.AlarmRaised += OnAlarmRaised;
        _alarmEngine.AlarmRecovered += OnAlarmRecovered;

        // 初始化：加载当前活跃报警
        LoadActiveAlarms();
    }

    /// <summary>报警列表（新的在前）</summary>
    public ObservableCollection<AlarmDisplayItem> Alarms { get; } = new();

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public int ActiveCount
    {
        get => _activeCount;
        set => SetProperty(ref _activeCount, value);
    }

    private void LoadActiveAlarms()
    {
        foreach (var evt in _alarmEngine.GetActiveAlarms())
            Alarms.Insert(0, new AlarmDisplayItem(evt));
        RefreshStatus();
    }

    private void OnAlarmRaised(AlarmEvent evt)
    {
        _dispatcher.BeginInvoke(() =>
        {
            Alarms.Insert(0, new AlarmDisplayItem(evt));
            RefreshStatus();
            _logService.Error($"报警: {evt.Message}", "AlarmWindow");
        });
    }

    private void OnAlarmRecovered(AlarmEvent evt)
    {
        _dispatcher.BeginInvoke(() =>
        {
            var item = Alarms.FirstOrDefault(a => a.PointKey == evt.PointKey);
            if (item != null)
            {
                item.IsRecovered = true;
                item.RecoverTime = evt.RecoverTime;
                item.RowBackground = "#1A22C55E"; // 恢复绿色
            }
            RefreshStatus();
        });
    }

    private void RefreshStatus()
    {
        ActiveCount = Alarms.Count(a => !a.IsRecovered);
        StatusText = ActiveCount > 0
            ? $"有 {ActiveCount} 个活跃报警"
            : "暂无报警";
    }

    /// <summary>窗口关闭时取消订阅</summary>
    public void Cleanup()
    {
        _alarmEngine.AlarmRaised -= OnAlarmRaised;
        _alarmEngine.AlarmRecovered -= OnAlarmRecovered;
    }
}

/// <summary>报警列表显示项</summary>
public class AlarmDisplayItem : BindableBase
{
    public string PointKey { get; }
    public string DeviceName { get; }
    public string PointName { get; }
    public double Value { get; }
    public AlarmLevel Level { get; }
    public string LevelText => Level == AlarmLevel.Extreme ? "紧急" : "警告";
    public string LevelBrush => Level == AlarmLevel.Extreme ? "#EF4444" : "#F59E0B";
    public string Message { get; }
    public DateTime TriggerTime { get; }

    private bool _isRecovered;
    public bool IsRecovered
    {
        get => _isRecovered;
        set
        {
            if (SetProperty(ref _isRecovered, value))
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(StatusText)));
        }
    }

    /// <summary>报警状态文本</summary>
    public string StatusText => IsRecovered ? "已恢复" : "报警中";

    private DateTime? _recoverTime;
    public DateTime? RecoverTime
    {
        get => _recoverTime;
        set => SetProperty(ref _recoverTime, value);
    }

    private string _rowBackground = "#1AEF4444";
    public string RowBackground
    {
        get => _rowBackground;
        set => SetProperty(ref _rowBackground, value);
    }

    public AlarmDisplayItem(AlarmEvent evt)
    {
        PointKey = evt.PointKey;
        DeviceName = evt.DeviceId;
        PointName = evt.PointName;
        Value = evt.Value;
        Level = evt.Level;
        Message = evt.Message;
        TriggerTime = evt.TriggerTime;
        IsRecovered = evt.IsRecovered;
        RecoverTime = evt.RecoverTime;
        if (IsRecovered) RowBackground = "#1A22C55E";
    }
}
