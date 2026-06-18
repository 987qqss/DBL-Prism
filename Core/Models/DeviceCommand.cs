using System.Collections.ObjectModel;

namespace Core.Models
{
    public class DeviceCommand
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        public string DeviceId { get; set; } = string.Empty;
        
        public string? DataPointId { get; set; }
        
        public CommandType CommandType { get; set; } = CommandType.Read;
        
        public byte OperationCode { get; set; } = 0x00;
        
        public ushort Address { get; set; } = 0;
        public ushort Length { get; set; } = 1;
        
        public object? WriteValue { get; set; }
        
        public DataFormat DataFormat { get; set; } = DataFormat.Int16;
        public float Scale { get; set; } = 1.0f;
        public float Offset { get; set; } = 0.0f;
        public string Unit { get; set; } = string.Empty;
        
        public string RequestData { get; set; } = string.Empty;
        
        public bool IsSystemCommand { get; set; } = false;
        
        public ObservableCollection<CommandParameter> Parameters { get; } = new();
    }

    public class CommandExecutionResult
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FormattedResult { get; set; }
        public long ExecutionTime { get; set; }
    }

    public enum CommandType
    {
        Read,
        Write,
        ReadWrite,
        Custom
    }

    public enum DataFormat
    {
        UInt16,
        Int16,
        UInt32,
        Int32,
        Float,
        Double,
        Bool,
        String,
        ByteArray
    }

    public class CommandParameter
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
