using System.Windows;
using System.Windows.Controls;
using DeviceModule.ViewModels;

namespace DeviceModule.Views
{
    /// <summary>
    /// ProductionLineDialogView.xaml 的交互逻辑
    /// </summary>
    public partial class ProductionLineDialogView : UserControl
    {
        public ProductionLineDialogView()
        {
            InitializeComponent();

            // 订阅视图模型的 CloseAction 事件来关闭窗口
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProductionLineDialogViewModel viewModel)
            {
                // 设置关闭动作，以便 ViewModel 可以控制窗口关闭
                var window = Window.GetWindow(this);
                viewModel.CloseAction = result =>
                {
                    if (window != null)
                    {
                        window.DialogResult = result;
                        window.Close();
                    }
                };
            }
        }
    }
}
