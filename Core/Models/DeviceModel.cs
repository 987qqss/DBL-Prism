using System.Collections.ObjectModel;

namespace Core.Models
{
    //设备模型
    public class DeviceModel
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string ProductionLineId { get; set; } = string.Empty;
        public ProtocolType ProtocolType { get; set; }
        
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; } = 502;
        
        public string SerialPortName { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public string Parity { get; set; } = "None";
        public string StopBits { get; set; } = "One";
        
        public int S7Rack { get; set; } = 0;
        public int S7Slot { get; set; } = 2;
        
        public byte SlaveId { get; set; } = 1;
        public int Timeout { get; set; } = 3000;
        public int RetryCount { get; set; } = 3;
        
        public bool IsConnected { get; set; } = false;
        public DeviceStatus Status { get; set; } = DeviceStatus.NotConfigured;
        
        public ObservableCollection<DeviceCommand> Commands { get; } = new();
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime UpdatedTime { get; set; } = DateTime.Now;
    }

    public enum DeviceStatus
    {
        Online,
        Offline,
        NotConfigured,
        Error
    }
}
