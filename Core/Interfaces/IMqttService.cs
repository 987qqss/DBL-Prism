using Core.Models.ProtocolModel;

namespace Core.Interfaces
{
    /// <summary>
    /// MQTT 消息服务接口 —— 提供与 MQTT Broker 的连接、订阅、发布能力。
    /// 用于储能系统设备数据上报、远程指令下发等 IoT 场景。
    /// 实现位于 Infrastructure.Services.MqttService。
    /// </summary>
    public interface IMqttService
    {
        /// <summary>是否已与 Broker 建立连接</summary>
        bool IsConnected { get; }

        /// <summary>
        /// 收到订阅消息时触发。
        /// 参数1: 消息主题 (topic)
        /// 参数2: 消息负载文本 (payload)
        /// </summary>
        event Action<string, string>? MessageReceived;

        /// <summary>与 Broker 连接成功时触发</summary>
        event Action? Connected;

        /// <summary>与 Broker 连接断开时触发，参数为断开原因描述</summary>
        event Action<string>? Disconnected;

        /// <summary>
        /// 连接到 MQTT Broker。
        /// </summary>
        /// <param name="config">MQTT 连接配置</param>
        /// <returns>连接是否成功</returns>
        Task<bool> ConnectAsync(MQTTModel config);

        /// <summary>正常断开与 Broker 的连接（发送 DISCONNECT 包）</summary>
        Task DisconnectAsync();

        /// <summary>
        /// 订阅指定主题的消息。
        /// </summary>
        /// <param name="topic">主题过滤器，支持通配符 + 和 #</param>
        /// <param name="qos">服务质量等级: 0=最多一次, 1=至少一次, 2=恰好一次</param>
        Task SubscribeAsync(string topic, int qos = 0);

        /// <summary>
        /// 取消订阅指定主题。
        /// </summary>
        /// <param name="topic">要取消订阅的主题</param>
        Task UnsubscribeAsync(string topic);

        /// <summary>
        /// 向指定主题发布消息。
        /// </summary>
        /// <param name="topic">目标主题</param>
        /// <param name="payload">消息负载文本</param>
        /// <param name="qos">服务质量等级: 0=最多一次, 1=至少一次, 2=恰好一次</param>
        /// <param name="retain">是否为保留消息（新订阅者连接后能立即收到）</param>
        /// <returns>发布是否成功</returns>
        Task<bool> PublishAsync(string topic, string payload, int qos = 0, bool retain = false);
    }
}
