using Core.Interfaces;

namespace Core.Models
{
    public class CustomProtocolModel : IProtocolConfig
    {
        public ProtocolType ProtocolType => ProtocolType.Custom;
        
        public string ConfigJson { get; set; } = "{}";
        public int Timeout { get; set; } = 3000;
        public int RetryCount { get; set; } = 3;
        
        public void Validate()
        {
            if (string.IsNullOrEmpty(ConfigJson))
                throw new ArgumentException("配置JSON不能为空");
        }
    }
}
