namespace Core.Models
{
    /// <summary>设备读取命令执行结果</summary>
    public class DeviceReadResult
    {
        public bool Success { get; set; }
        public string CommandName { get; set; } = string.Empty;
        public object? RawValue { get; set; }
        public double? ConvertedValue { get; set; }
        public string FormattedValue { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>设备写入命令执行结果</summary>
    public class DeviceWriteResult
    {
        public bool Success { get; set; }
        public string CommandName { get; set; } = string.Empty;
        public object? WrittenValue { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
