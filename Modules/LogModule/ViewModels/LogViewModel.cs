using Prism.Mvvm;
using Prism.Commands;
using System.Collections.ObjectModel;

namespace LogModule.ViewModels
{
    public class LogViewModel : BindableBase
    {
        private string _logLevel = "全部";
        private string _searchKeyword = string.Empty;

        public string LogLevel
        {
            get => _logLevel;
            set => SetProperty(ref _logLevel, value);
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set => SetProperty(ref _searchKeyword, value);
        }

        public ObservableCollection<LogItem> Logs { get; } = new();

        public DelegateCommand ExportLogCommand { get; }
        public DelegateCommand ClearLogCommand { get; }

        public LogViewModel()
        {
            ExportLogCommand = new DelegateCommand(ExportLog);
            ClearLogCommand = new DelegateCommand(ClearLog);

            InitializeMockData();
        }

        private void InitializeMockData()
        {
            Logs.Add(new LogItem { Time = "10:30:01", Level = "INFO", Message = "系统启动成功" });
            Logs.Add(new LogItem { Time = "10:30:02", Level = "INFO", Message = "用户 admin 登录成功" });
            Logs.Add(new LogItem { Time = "10:30:05", Level = "WARN", Message = "温度超过预警阈值" });
            Logs.Add(new LogItem { Time = "10:30:10", Level = "INFO", Message = "开始数据采集" });
            Logs.Add(new LogItem { Time = "10:30:15", Level = "ERROR", Message = "通信异常，正在重试" });
            Logs.Add(new LogItem { Time = "10:30:16", Level = "INFO", Message = "通信恢复正常" });
            Logs.Add(new LogItem { Time = "10:31:00", Level = "INFO", Message = "数据采集完成" });
            Logs.Add(new LogItem { Time = "10:31:30", Level = "WARN", Message = "湿度偏高" });
        }

        private void ExportLog()
        {
        }

        private void ClearLog()
        {
            Logs.Clear();
        }
    }

    public class LogItem
    {
        public string Time { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}