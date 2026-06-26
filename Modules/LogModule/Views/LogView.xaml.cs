using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace LogModule.Views
{
    public partial class LogView
    {
        public LogView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ViewModels.LogViewModel oldVm)
                oldVm.Logs.CollectionChanged -= OnLogsChanged;

            if (e.NewValue is ViewModels.LogViewModel newVm)
            {
                newVm.Logs.CollectionChanged += OnLogsChanged;
                // 初始化加载后滚动到底部
                ScrollToEnd();
            }
        }

        private void OnLogsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ScrollToEnd();
            }
        }

        private void ScrollToEnd()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (LogListBox.Items.Count > 0)
                    LogListBox.ScrollIntoView(LogListBox.Items[^1]);
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void LogListBox_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollToEnd();
        }
    }
}
