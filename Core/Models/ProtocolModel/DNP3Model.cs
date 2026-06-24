using Core.Interfaces;

namespace Core.Models
{
    public class DNP3Model : IProtocolConfig
    {
        public ProtocolType ProtocolType => ProtocolType.Dnp3;
        
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; } = 20000;
        public int MasterAddress { get; set; } = 1;
        public int SlaveAddress { get; set; } = 1;
        public int Timeout { get; set; } = 5000;
        public int RetryCount { get; set; } = 3;
        
        public void Validate()
        {
            if (string.IsNullOrEmpty(IpAddress))
                throw new ArgumentException("IP地址不能为空");
        }
    }
}
