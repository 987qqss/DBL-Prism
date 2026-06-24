using Core.Interfaces;

namespace Core.Models
{
    public class S7Model : IProtocolConfig
    {
        public ProtocolType ProtocolType => ProtocolType.S7;
        
        public string IpAddress { get; set; } = string.Empty;
        public int Rack { get; set; } = 0;
        public int Slot { get; set; } = 2;
        public int Timeout { get; set; } = 3000;
        public int RetryCount { get; set; } = 3;
        
        public void Validate()
        {
            if (string.IsNullOrEmpty(IpAddress))
                throw new ArgumentException("IP地址不能为空");
        }
    }
}
