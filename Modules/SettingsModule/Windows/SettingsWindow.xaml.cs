using System.Windows;
using SettingsModule.ViewModels;

namespace SettingsModule.Windows;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
