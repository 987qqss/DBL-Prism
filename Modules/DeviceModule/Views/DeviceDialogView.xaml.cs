using System.Windows;
using System.Windows.Controls;
using DeviceModule.ViewModels;

namespace DeviceModule.Views
{
    public partial class DeviceDialogView : UserControl
    {
        public DeviceDialogView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is DeviceDialogViewModel viewModel)
            {
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
