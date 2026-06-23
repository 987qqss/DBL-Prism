using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IDevice
    {
        string Id { get; }
        string Name { get; set; }
        ProtocolType ProtocolType { get; }
        DeviceStatus Status { get; set; }
        bool IsConnected { get; set; }
        string ProductionLineId { get; set; }
        
        Task<bool> Connect();
        Task Disconnect();
        Task<bool> TestConnection();
    }

    public interface IProtocolConfig
    {
        ProtocolType ProtocolType { get; }
        void Validate();
    }

    public enum ProtocolType
    {
        ModbusTcp,
        ModbusRtu,
        S7,
        TcpIp,
        OpcUa,
        Dnp3,
        Bacnet,
        Scpi,
        Custom
    }

    public enum DeviceStatus
    {
        Online,
        Offline,
        NotConfigured,
        Error
    }

    public enum SerialParity
    {
        None,
        Odd,
        Even,
        Mark,
        Space
    }

    public enum SerialStopBits
    {
        One,
        OnePointFive,
        Two
    }
}
