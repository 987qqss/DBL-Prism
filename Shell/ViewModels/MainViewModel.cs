using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System;

namespace Shell.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private string _title = "储能集装箱电池仓数据监测系统";
        private string _statusMessage = "系统就绪";
        private bool _isSidebarOpen = false;
        private string _currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsSidebarOpen
        {
            get => _isSidebarOpen;
            set => SetProperty(ref _isSidebarOpen, value);
        }

        public string CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        public DelegateCommand MinimizeCommand { get; }
        public DelegateCommand MaximizeCommand { get; }
        public DelegateCommand CloseCommand { get; }
        public DelegateCommand ToggleSidebarCommand { get; }

        public MainViewModel()
        {
            MinimizeCommand = new DelegateCommand(() => SystemCommands_Minimize());
            MaximizeCommand = new DelegateCommand(() => SystemCommands_Maximize());
            CloseCommand = new DelegateCommand(() => SystemCommands_Close());
            ToggleSidebarCommand = new DelegateCommand(() => IsSidebarOpen = !IsSidebarOpen);

            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += (s, e) => CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            timer.Start();
        }

        private void SystemCommands_Minimize()
        {
            if (System.Windows.Application.Current?.MainWindow is System.Windows.Window w)
                w.WindowState = System.Windows.WindowState.Minimized;
        }

        private void SystemCommands_Maximize()
        {
            if (System.Windows.Application.Current?.MainWindow is System.Windows.Window w)
            {
                w.WindowState = w.WindowState == System.Windows.WindowState.Maximized
                    ? System.Windows.WindowState.Normal
                    : System.Windows.WindowState.Maximized;
            }
        }

        private void SystemCommands_Close()
        {
            System.Windows.Application.Current?.Shutdown();
        }
    }
}