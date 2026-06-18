namespace Core.Models
{
    public class ModbusReadResult
    {
        public byte SlaveId { get; init; }
        public ushort StartAddress { get; init; }
        public ushort[] RegisterValues { get; init; } = new ushort[0];
        public bool[] CoilValues { get; init; } = new bool[0];
        public ModbusFunctionCode Function { get; init; }
    }

    public enum ModbusFunctionCode
    {
        ReadCoils = 0x01,
        ReadDiscreteInputs = 0x02,
        ReadHoldingRegisters = 0x03,
        ReadInputRegisters = 0x04,
        WriteSingleCoil = 0x05,
        WriteSingleRegister = 0x06,
        WriteMultipleCoils = 0x0F,
        WriteMultipleRegisters = 0x10
    }
}
