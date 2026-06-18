using Core.Interfaces;

namespace Core.Models
{
    public class BACnetModel : IProtocolConfig
    {
        public ProtocolType ProtocolType => ProtocolType.Bacnet;
        
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; } = 47808;
        public int Timeout { get; set; } = 5000;
        public int RetryCount { get; set; } = 3;
        
        public void Validate()
        {
            if (string.IsNullOrEmpty(IpAddress))
                throw new ArgumentException("IP地址不能为空");
        }
    }
}
