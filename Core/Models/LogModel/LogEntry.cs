namespace Core.Models.LogModel
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public LogLevel Level { get; set; } = LogLevel.Info;
        public string Message { get; set; } = string.Empty;
        public string Source { get; set; } = "System";
        public string? StackTrace { get; set; }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error
    }
}
