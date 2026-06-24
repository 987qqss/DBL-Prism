using System.Windows;
using System.Windows.Controls;
using DeviceModule.ViewModels;

namespace DeviceModule.Views
{
    public partial class CommandDialogView : UserControl
    {
        public CommandDialogView()
        {
            InitializeComponent();
            this.DataContext = new CommandDialogViewModel();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CommandDialogViewModel viewModel)
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
