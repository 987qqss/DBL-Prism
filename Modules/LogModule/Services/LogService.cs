using Core.Interfaces;
using Core.Models.LogModel;
using Core.Events;
using Prism.Events;
using NLog;
using NLogLevel = NLog.LogLevel;
using System.Collections.Concurrent;

namespace LogModule.Services
{
    public class LogService : ILogService
    {
        //ConcurrentQueue是一个无界队列通道，作用是提供一个全局访问的日志缓存
        private readonly ConcurrentQueue<LogEntry> _logs = new();
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public LogService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public void Info(string message, string source = "System") => Log(Core.Models.LogModel.LogLevel.Info, message, source);

        public void Warn(string message, string source = "System") => Log(Core.Models.LogModel.LogLevel.Warn, message, source);

        public void Error(string message, string source = "System", Exception? exception = null) => Log(Core.Models.LogModel.LogLevel.Error, message, source, exception);

        public void Debug(string message, string source = "System") => Log(Core.Models.LogModel.LogLevel.Debug, message, source);

        private void Log(Core.Models.LogModel.LogLevel level, string message, string source, Exception? exception = null)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                Source = source,
                StackTrace = exception?.ToString()
            };

            _logs.Enqueue(entry);//放入队列
            TrimLogs();
            _eventAggregator.GetEvent<LogAddedEvent>().Publish(entry);//发布事件

            switch (level)
            {
                case Core.Models.LogModel.LogLevel.Debug:
                    _logger.Debug($"[{source}] {message}");//写入到本地文件
                    break;
                case Core.Models.LogModel.LogLevel.Info:
                    _logger.Info($"[{source}] {message}");
                    break;
                case Core.Models.LogModel.LogLevel.Warn:
                    _logger.Warn($"[{source}] {message}");
                    break;
                case Core.Models.LogModel.LogLevel.Error:
                    _logger.Error(exception, $"[{source}] {message}");
                    break;
            }
        }

        public IReadOnlyList<LogEntry> GetLogs(int count = 100) => _logs.Reverse().Take(count).Reverse().ToList().AsReadOnly();

        public void ClearLogs() => _logs.Clear();

        public void ExportLogs(string filePath)//导出日志
        {
            var sb = new System.Text.StringBuilder();
            foreach (var log in _logs)
            {
                sb.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss} [{log.Level}] [{log.Source}] {log.Message}");
                if (!string.IsNullOrEmpty(log.StackTrace))
                    sb.AppendLine(log.StackTrace);
            }
            System.IO.File.WriteAllText(filePath, sb.ToString());
        }

        private void TrimLogs()
        {
            const int maxLogs = 5000;
            while (_logs.Count > maxLogs)
                _logs.TryDequeue(out _);
        }
    }
}