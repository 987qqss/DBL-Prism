using MonitorModule.ViewModels;
using MonitorModule.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace MonitorModule;

/// <summary>设备监控模块 —— 数据点可视化（实时曲线/指示灯/文本）</summary>
public class DeviceMonitorModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<DeviceMonitorView, DeviceMonitorViewModel>();
        // transient：每次打开监控窗口解析新实例（窗口关闭 Cleanup 后重新订阅）
        containerRegistry.Register<DeviceMonitorViewModel>();
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
    }
}
