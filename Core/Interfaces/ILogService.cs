using Core.Models.LogModel;

namespace Core.Interfaces
{
    public interface ILogService
    {
        void Info(string message, string source = "System");
        void Warn(string message, string source = "System");
        void Error(string message, string source = "System", Exception? exception = null);
        void Debug(string message, string source = "System");
        IReadOnlyList<LogEntry> GetLogs(int count = 100);
        void ClearLogs();
        void ExportLogs(string filePath);
    }
}
