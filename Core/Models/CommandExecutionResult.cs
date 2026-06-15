namespace Core.Models
{
    //设备命令返回结果
    public class CommandExecutionResult
    {
        public string CommandId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public bool Success { get; set; } = false;
        public string? ErrorMessage { get; set; }
        public object? Result { get; set; }
        public string? FormattedResult { get; set; }
        public DateTime ExecutionTime { get; set; } = DateTime.Now;
        public long DurationMs { get; set; } = 0;
    }
}
