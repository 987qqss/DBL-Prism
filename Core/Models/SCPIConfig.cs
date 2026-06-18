﻿using Core.Interfaces;

namespace Core.Models
{
    public class SCPIConfig : IProtocolConfig
    {
        public ProtocolType ProtocolType => ProtocolType.Scpi;

        public string IpAddress { get; set; } = string.Empty;
        public int Port { get; set; } = 5025;
        public int Timeout { get; set; } = 5000;
        public int RetryCount { get; set; } = 3;
        public string Terminator { get; set; } = "\n";
        public bool UseVisa { get; set; } = false;
        public string VisaResourceString { get; set; } = string.Empty;

        public void Validate()
        {
            if (string.IsNullOrEmpty(IpAddress))
                throw new ArgumentException("IP地址不能为空");
            if (Port <= 0 || Port > 65535)
                throw new ArgumentException("端口号无效");
            if (UseVisa && string.IsNullOrEmpty(VisaResourceString))
                throw new ArgumentException("VISA资源字符串不能为空");
        }
    }
}
