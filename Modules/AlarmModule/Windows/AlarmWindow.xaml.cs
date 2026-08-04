using System.Windows;
using AlarmModule.ViewModels;
using AlarmModule.Views;

namespace AlarmModule.Windows;

public partial class AlarmWindow : Window
{
    public AlarmWindow(AlarmViewModel viewModel)
    {
        InitializeComponent();
        Content = new AlarmView { DataContext = viewModel };
        Closed += (_, _) => viewModel.Cleanup();
    }
}
