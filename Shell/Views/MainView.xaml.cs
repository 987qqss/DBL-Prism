using System.Windows;
using System.Windows.Input;

namespace Shell.Views
{
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();

            // 标题栏空白处双击最大化
            MouseDoubleClick += MainView_MouseDoubleClick;
        }

        private void MainView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 双击标题栏区域才触发
            if (e.GetPosition(this).Y <= 32)
            {
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            }
        }
    }
}