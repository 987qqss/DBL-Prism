using DeviceModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace DeviceModule.Views
{
    public partial class DeviceCommandTreeView
    {
        public DeviceCommandTreeView()
        {
            InitializeComponent();
            CommandTree.SelectedItemChanged += OnTreeSelectionChanged;
        }

        private void OnTreeSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is DeviceCommandTreeViewModel vm)
            {
                vm.SelectedItem = e.NewValue;
            }
        }
    }
}
