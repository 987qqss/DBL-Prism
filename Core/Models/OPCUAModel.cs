using Core.Interfaces;

namespace Core.Models
{
    //OPC模型
    public class OPCUAModel : IProtocolConfig
    {
        public ProtocolType ProtocolType => ProtocolType.OpcUa;
        
        public string ServerUrl { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Timeout { get; set; } = 10000;
        public int RetryCount { get; set; } = 3;
        
        public void Validate()
        {
            if (string.IsNullOrEmpty(ServerUrl))
                throw new ArgumentException("服务器URL不能为空");
        }
    }
}
