using System.Collections.ObjectModel;
using System.Windows.Threading;
using Core.Interfaces;
using Core.Models;
using Prism.Mvvm;

namespace MonitorModule.ViewModels;

/// <summary>
/// 设备监控面板 ViewModel —— 遍历数据点配置，为每个点创建展示模型并订阅。
/// 监控界面用 ItemsControl + DataTemplateSelector 自动选择模板。
/// </summary>
public class DeviceMonitorViewModel : BindableBase
{
    private readonly IDataPointStore _store;
    private readonly IDataPointConfigService _configService;
    private readonly ILogService _logService;
    private readonly Dispatcher _dispatcher;
    private readonly List<IDataPointSubscriber> _subscribers = new();

    private string _statusText = "监控就绪";
    private bool _hasDataPoints;

    public DeviceMonitorViewModel(
        IDataPointStore store,
        IDataPointConfigService configService,
        ILogService logService)
    {
        _store = store;
        _configService = configService;
        _logService = logService;
        _dispatcher = System.Windows.Application.Current?.Dispatcher
            ?? Dispatcher.CurrentDispatcher;

        BuildDataPoints();
    }

    /// <summary>所有数据点的展示模型（绑定到 ItemsControl）</summary>
    public ObservableCollection<DataPointDisplayModel> DataPoints { get; } = new();

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public bool HasDataPoints
    {
        get => _hasDataPoints;
        set => SetProperty(ref _hasDataPoints, value);
    }

    /// <summary>遍历数据点配置，创建展示模型并订阅</summary>
    private void BuildDataPoints()
    {
        DataPoints.Clear();
        _subscribers.Clear();

        var configs = _configService.Configs.Where(c => c.Enabled).ToList();

        foreach (var cfg in configs)
        {
            var model = new DataPointDisplayModel(cfg, _dispatcher);
            DataPoints.Add(model);
            _subscribers.Add(model);
            _store.Subscribe(model);
        }

        HasDataPoints = DataPoints.Count > 0;
        StatusText = HasDataPoints
            ? $"正在监控 {DataPoints.Count} 个数据点"
            : "暂无监测数据点，请在设备命令树勾选\"是否监控\"";

        _logService.Info($"设备监控面板已加载 ({DataPoints.Count} 个数据点)", "Monitor");
    }

    /// <summary>界面销毁/退出时取消订阅</summary>
    public void Cleanup()
    {
        foreach (var sub in _subscribers)
            _store.Unsubscribe(sub);
        _subscribers.Clear();
    }
}
