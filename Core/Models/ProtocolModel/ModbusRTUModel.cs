using Core.Interfaces;

namespace Core.Models
{
    /// <summary>Modbus RTU 串口通讯配置</summary>
    public class ModbusRTUModel : IProtocolConfig
    {
        public ProtocolType ProtocolType => ProtocolType.ModbusRtu;

        public string SerialPortName { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public SerialParity Parity { get; set; } = SerialParity.None;
        public SerialStopBits StopBits { get; set; } = SerialStopBits.One;
        public byte SlaveId { get; set; } = 1;
        public int Timeout { get; set; } = 3000;
        public int RetryCount { get; set; } = 3;

        public void Validate()
        {
            if (string.IsNullOrEmpty(SerialPortName))
                throw new ArgumentException("串口名称不能为空");
        }
    }
}
