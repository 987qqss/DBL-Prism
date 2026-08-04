using System.Windows.Controls;
using MonitorModule.ViewModels;

namespace MonitorModule.Views;

public partial class DeviceMonitorView : UserControl
{
    public DeviceMonitorView()
    {
        InitializeComponent();
    }

    /// <summary>导航离开时取消订阅，避免内存泄漏</summary>
    private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is DeviceMonitorViewModel vm)
            vm.Cleanup();
    }
}
