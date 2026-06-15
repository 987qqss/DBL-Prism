namespace Core.Models
{
    //设备通讯协议类型
    public enum ProtocolType
    {
        ModbusRtu,
        ModbusTcp,
        S7,
        TcpIp,
        OpcUa,
        Dnp3,
        Bacnet,
        Custom
    }
}
