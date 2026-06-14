namespace Core.Models
{
    public class ModbusWriteCommand
    {
        public byte SlaveId { get; init; }
        public ushort StartAddress { get; init; }
        public object? Value { get; init; }
        public ModbusFunctionCode Function { get; init; }
        public TaskCompletionSource<bool> Completion { get; init; } = new();
    }
}