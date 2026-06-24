using System.Text.Json.Serialization;
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

    /// <summary>协议配置接口 —— 通过 [JsonDerivedType] 实现多态序列化</summary>
    [JsonDerivedType(typeof(Models.ModbusTCPModel), nameof(ProtocolType.ModbusTcp))]
    [JsonDerivedType(typeof(Models.ModbusRTUModel), nameof(ProtocolType.ModbusRtu))]
    [JsonDerivedType(typeof(Models.S7Model), nameof(ProtocolType.S7))]
    [JsonDerivedType(typeof(Models.TCPIPModel), nameof(ProtocolType.TcpIp))]
    [JsonDerivedType(typeof(Models.OPCUAModel), nameof(ProtocolType.OpcUa))]
    [JsonDerivedType(typeof(Models.DNP3Model), nameof(ProtocolType.Dnp3))]
    [JsonDerivedType(typeof(Models.BACnetModel), nameof(ProtocolType.Bacnet))]
    [JsonDerivedType(typeof(Models.SCPIConfig), nameof(ProtocolType.Scpi))]
    [JsonDerivedType(typeof(Models.CustomProtocolModel), nameof(ProtocolType.Custom))]
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
