using System.Windows;
using MonitorModule.ViewModels;
using Prism.Ioc;

namespace MonitorModule.Windows;

public partial class MonitorWindow : Window
{
    public MonitorWindow(DeviceMonitorViewModel viewModel)
    {
        InitializeComponent();

        // 把监控面板 ViewModel 设为窗口内容的数据上下文
        var monitorView = new Views.DeviceMonitorView { DataContext = viewModel };
        Content = monitorView;

        // 窗口关闭时清理订阅
        Closed += (_, _) => viewModel.Cleanup();
    }
}
