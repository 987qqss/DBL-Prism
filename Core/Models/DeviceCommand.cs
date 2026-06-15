using System.Collections.ObjectModel;

namespace Core.Models
{
    public class DeviceCommand
    {
        //设备编号
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DeviceId { get; set; } = string.Empty;//设备ID
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;//设备描述
        public CommandType CommandType { get; set; }//命令类型
        
        //Modbus配置
        public byte SlaveId { get; set; } = 1;
        public ushort StartAddress { get; set; } = 0;
        public ushort Quantity { get; set; } = 1;
        public ModbusFunctionCode FunctionCode { get; set; }
        
        //S7配置
        public int S7DbNumber { get; set; } = 1;
        public int S7StartOffset { get; set; } = 0;
        public int S7Length { get; set; } = 4;
        public S7DataType S7DataType { get; set; } = S7DataType.Int;
        
        //TCP配置
        public string TcpCommand { get; set; } = string.Empty;
        public string TcpResponsePattern { get; set; } = string.Empty;
        
        public DataFormat DataFormat { get; set; } = DataFormat.UInt16;
        public float Scale { get; set; } = 1.0f;
        public float Offset { get; set; } = 0.0f;
        public string Unit { get; set; } = string.Empty;
        
        public bool IsSystemCommand { get; set; } = false;
        
        public ObservableCollection<CommandParameter> Parameters { get; } = new();
    }

    public enum CommandType
    {
        ModbusRead,
        ModbusWrite,
        S7Read,
        S7Write,
        TcpRequest,
        Custom
    }

    public enum S7DataType
    {
        Bit,
        Byte,
        Int,
        DInt,
        Real,
        String
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
        String
    }
}
