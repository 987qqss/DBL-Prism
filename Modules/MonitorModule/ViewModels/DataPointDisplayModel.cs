using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using Core.Interfaces;
using Core.Models;
using Infrastructure.Services;
using Prism.Mvvm;

namespace MonitorModule.ViewModels;

/// <summary>
/// 单个数据点的展示模型 —— 既是订阅者，又是 UI 绑定对象。
/// 按 DataPointDisplayType 将数据分发到曲线/指示灯/文本展示。
/// </summary>
public class DataPointDisplayModel : BindableBase, IDataPointSubscriber
{
    private readonly Dispatcher _dispatcher;
    private const int MaxSamples = 300; // 曲线最多保留 300 个样本

    public string PointKey { get; }
    public string DeviceId { get; }
    public string PointName { get; }
    public string Unit { get; }
    public DataPointDisplayType DisplayType { get; }

    // ─── 曲线数据（数值型） ───
    public ObservableCollection<double> TrendValues { get; } = new();

    // 归一化坐标字符串，供 Polyline 绑定（格式 "x,y x,y ..."）
    private string _trendPoints = "";
    public string TrendPoints
    {
        get => _trendPoints;
        private set => SetProperty(ref _trendPoints, value);
    }

    // ─── 指示灯数据（布尔型） ───
    private bool _isOn;
    public bool IsOn
    {
        get => _isOn;
        set => SetProperty(ref _isOn, value);
    }

    public string OnOffText => IsOn ? "ON" : "OFF";

    // ─── 文本/当前值 ───
    private string _currentText = "--";
    public string CurrentText
    {
        get => _currentText;
        set => SetProperty(ref _currentText, value);
    }

    // ─── 报警状态（所有类型通用） ───
    private bool _isAlarm;
    public bool IsAlarm
    {
        get => _isAlarm;
        set => SetProperty(ref _isAlarm, value);
    }

    private string _alarmLevelText = "";
    public string AlarmLevelText
    {
        get => _alarmLevelText;
        set => SetProperty(ref _alarmLevelText, value);
    }

    public DataPointDisplayModel(DataPointConfig config, Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
        PointKey = DataPointStore.BuildKey(config.DeviceId, config.PointName);
        DeviceId = config.DeviceId;
        PointName = config.PointName;
        Unit = config.Unit;
        DisplayType = DataPointDisplayResolver.Resolve(config.DataFormat);
    }

    /// <summary>订阅回调（工作线程）—— 用 Dispatcher 切到 UI 线程更新</summary>
    public void OnDataPointUpdated(DataPointValue value)
    {
        if (value.PointKey != PointKey) return;

        _dispatcher.BeginInvoke(() =>
        {
            // 报警状态（通用）
            IsAlarm = value.IsAlarm;
            AlarmLevelText = value.AlarmLevel switch
            {
                AlarmLevel.Extreme => "紧急",
                AlarmLevel.Warning => "警告",
                _ => ""
            };

            switch (DisplayType)
            {
                case DataPointDisplayType.TrendChart:
                    UpdateTrend(value);
                    break;

                case DataPointDisplayType.Indicator:
                    IsOn = value.RawValue is bool b ? b : (value.NumericValue ?? 0) != 0;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(OnOffText)));
                    CurrentText = value.FormattedValue;
                    break;

                default:
                    CurrentText = value.FormattedValue;
                    break;
            }
        });
    }

    private void UpdateTrend(DataPointValue value)
    {
        var v = value.NumericValue ?? 0;
        TrendValues.Add(v);
        if (TrendValues.Count > MaxSamples)
            TrendValues.RemoveAt(0);

        // 归一化为 Polyline 坐标（宽 100 高 40，纵向留 5 上下边距）
        int n = TrendValues.Count;
        if (n == 0) { TrendPoints = ""; return; }

        double min = TrendValues.Min();
        double max = TrendValues.Max();
        double range = max - min;
        if (range < 1e-9) range = 1; // 防除零

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < n; i++)
        {
            double x = (double)i / (n - 1) * 100.0;
            double y = 40.0 - (TrendValues[i] - min) / range * 30.0 - 5.0;
            sb.Append(x.ToString("0.0")).Append(',').Append(y.ToString("0.0")).Append(' ');
        }
        TrendPoints = sb.ToString().TrimEnd();
        CurrentText = value.FormattedValue;
    }
}
