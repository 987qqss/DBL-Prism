namespace Core.Models.ProtocolModel
{
    /// <summary>
    /// MQTT 协议配置模型 —— 描述与 MQTT Broker 建立连接所需的全部参数。
    /// 由 IMqttService.ConnectAsync 使用，可通过 JSON 序列化持久化到配置文件。
    /// </summary>
    public class MQTTModel
    {
        /// <summary>MQTT Broker 服务器地址（IP 或域名），默认本地回环</summary>
        public string BrokerAddress { get; set; } = "127.0.0.1";

        /// <summary>Broker 端口号。明文默认 1883，TLS 默认 8883</summary>
        public int Port { get; set; } = 1883;

        /// <summary>客户端唯一标识。留空时由 Broker 自动分配随机 ID</summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>用户名（若 Broker 开启了身份验证则必填）</summary>
        public string? Username { get; set; }

        /// <summary>密码（若 Broker 开启了身份验证则必填）</summary>
        public string? Password { get; set; }

        /// <summary>是否启用 TLS/SSL 加密传输。启用后端口通常改为 8883</summary>
        public bool UseTls { get; set; } = false;

        /// <summary>
        /// 是否跳过 TLS 证书校验。默认 false（校验证书，更安全）。
        /// 仅在 Broker 使用自签名证书且无法导入信任链时设为 true。
        /// </summary>
        public bool IgnoreCertificateErrors { get; set; } = false;

        /// <summary>心跳保活间隔（秒）。在此周期内无消息收发则自动发送 PINGREQ</summary>
        public int KeepAliveSeconds { get; set; } = 60;

        /// <summary>是否为清除会话。true 表示连接时不恢复上次离线时的订阅与未达消息</summary>
        public bool CleanSession { get; set; } = true;

        // ===== 遗嘱消息（LWT）配置 =====
        // 当客户端非正常断开时，Broker 会自动向以下主题发布遗嘱消息

        /// <summary>遗嘱消息主题（可选，留空表示不设置遗嘱）</summary>
        public string? WillTopic { get; set; }

        /// <summary>遗嘱消息负载（可选）</summary>
        public string? WillPayload { get; set; }

        /// <summary>遗嘱消息 QoS 等级（0/1/2）</summary>
        public int WillQoS { get; set; } = 0;

        /// <summary>遗嘱消息是否保留在 Broker 端</summary>
        public bool WillRetain { get; set; } = false;

        /// <summary>连接超时时间（秒）</summary>
        public int ConnectTimeoutSeconds { get; set; } = 10;

        /// <summary>
        /// 校验配置参数合法性，不合法时抛出 ArgumentException。
        /// 在 ConnectAsync 调用前应先执行此方法。
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(BrokerAddress))
                throw new ArgumentException("MQTT Broker 地址不能为空");

            if (Port <= 0 || Port > 65535)
                throw new ArgumentException($"MQTT 端口号无效: {Port}");

            if (KeepAliveSeconds < 0)
                throw new ArgumentException("心跳间隔不能为负数");

            if (UseTls && Port == 1883)
                throw new ArgumentException("启用 TLS 时端口 1883 通常不正确，建议使用 8883");
        }
    }
}
