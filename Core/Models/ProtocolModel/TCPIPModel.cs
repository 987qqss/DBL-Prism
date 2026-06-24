using Core.Interfaces;

namespace Core.Models
{
    public class TCPIPModel : IProtocolConfig
    {
        public ProtocolType ProtocolType => ProtocolType.TcpIp;
        
        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; } = 80;
        public int Timeout { get; set; } = 3000;
        public int RetryCount { get; set; } = 3;
        
        public void Validate()
        {
            if (string.IsNullOrEmpty(IpAddress))
                throw new ArgumentException("IP地址不能为空");
            if (Port <= 0 || Port > 65535)
                throw new ArgumentException("端口号无效");
        }
    }
}
