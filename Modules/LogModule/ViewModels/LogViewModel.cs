using Prism.Mvvm;
using Prism.Commands;
using Prism.Events;
using Core.Models.LogModel;
using Core.Events;
using Core.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace LogModule.ViewModels
{
    public class LogViewModel : BindableBase
    {
        private readonly ILogService _logService;
        private readonly IEventAggregator _eventAggregator;
        private string _selectedLevel = "全部";
        private string _searchKeyword = string.Empty;
        private ObservableCollection<LogItem> _logs = new();

        public string SelectedLevel
        {
            get => _selectedLevel;
            set => SetProperty(ref _selectedLevel, value);
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set => SetProperty(ref _searchKeyword, value);
        }

        public ObservableCollection<LogItem> Logs
        {
            get => _logs;
            set => SetProperty(ref _logs, value);
        }

        public DelegateCommand ExportLogCommand { get; }
        public DelegateCommand ClearLogCommand { get; }

        public LogViewModel(ILogService logService, IEventAggregator eventAggregator)
        {
            _logService = logService;
            _eventAggregator = eventAggregator;

            ExportLogCommand = new DelegateCommand(ExportLog);
            ClearLogCommand = new DelegateCommand(ClearLog);

            LoadLogs();
            SubscribeToLogEvents();
        }

        private void LoadLogs()
        {
            var logs = _logService.GetLogs(1000);
            Logs = new ObservableCollection<LogItem>(logs.Select(l => new LogItem(l)));
        }

        private void SubscribeToLogEvents()
        {
            _eventAggregator.GetEvent<LogAddedEvent>().Subscribe(OnLogAdded);
        }

        private void OnLogAdded(LogEntry entry)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Logs.Insert(0, new LogItem(entry));
                if (Logs.Count > 1000)
                    Logs.RemoveAt(Logs.Count - 1);
            });
        }

        private void ExportLog()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "文本文件 (*.txt)|*.txt|CSV文件 (*.csv)|*.csv",
                FileName = $"Log_{DateTime.Now:yyyy-MM-dd_HHmmss}.txt"
            };

            if (dialog.ShowDialog() == true)
                _logService.ExportLogs(dialog.FileName);
        }

        private void ClearLog()
        {
            _logService.ClearLogs();
            Logs.Clear();
        }
    }

    public class LogItem
    {
        public string Time { get; }
        public LogLevel Level { get; }
        public string Message { get; }
        public string Source { get; }

        public LogItem(LogEntry entry)
        {
            Time = entry.Timestamp.ToString("HH:mm:ss.fff");
            Level = entry.Level;
            Message = entry.Message;
            Source = entry.Source;
        }
    }
}
