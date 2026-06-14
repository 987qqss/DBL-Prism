namespace Infrastructure.Communication
{
    public interface IModbusService
    {
        bool Connect(string ipAddress, int port);
        void Disconnect();
        bool IsConnected { get; }
        Task<ushort[]> ReadInputRegistersAsync(byte slaveId, ushort startAddress, ushort count);
        Task<ushort[]> ReadHoldingRegistersAsync(byte slaveId, ushort startAddress, ushort count);
        Task<bool[]> ReadCoilsAsync(byte slaveId, ushort startAddress, ushort count);
        Task<bool[]> ReadDiscreteInputsAsync(byte slaveId, ushort startAddress, ushort count);
        Task WriteSingleCoilAsync(byte slaveId, ushort address, bool value);
        Task WriteSingleRegisterAsync(byte slaveId, ushort address, ushort value);
        Task WriteMultipleCoilsAsync(byte slaveId, ushort startAddress, bool[] values);
        Task WriteMultipleRegistersAsync(byte slaveId, ushort startAddress, ushort[] values);
    }
}