using System.Threading.Tasks;

namespace Core.Models
{
    public class ModbusWriteCommand
    {
        public byte SlaveId { get; set; }
        public ushort StartAddress { get; set; }
        public ushort[] Values { get; set; } = new ushort[0];
        public ModbusFunctionCode Function { get; set; }
        public object? Value { get; set; }
        public TaskCompletionSource<bool> Completion { get; } = new TaskCompletionSource<bool>();
    }
}
