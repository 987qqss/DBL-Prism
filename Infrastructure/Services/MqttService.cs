using Core.Interfaces;
using Core.Models.ProtocolModel;
using MQTTnet;
using MQTTnet.Protocol;
using System.Text;

namespace Infrastructure.Services
{
    /// <summary>
    /// MQTT 消息服务 —— 基于 MQTTnet v5 实现的 MQTT 客户端服务。
    /// <para>职责: 连接/断开生命周期、主题订阅/取消、消息发布、断线自动重连、事件通知。</para>
    /// </summary>
    public class MqttService : IMqttService, IAsyncDisposable, IDisposable
    {
        private readonly ILogService _logService;

        // MQTTnet v5 客户端工厂，用于创建客户端实例和构建器
        private readonly MqttClientFactory _factory = new();

        // MQTT 客户端实例，连接后持有，断开/Dispose 后释放
        private IMqttClient? _client;

        // 最近一次使用的连接配置，用于断线重连
        private MQTTModel? _lastConfig;

        // 是否已主动断开（主动断开不重连）
        private bool _disconnectRequested;

        // 重连任务是否已在运行（防止并发重连）
        private int _reconnectInProgress;

        // 是否已释放
        private bool _disposed;

        // 意外断开后的重连延时
        private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);

        /// <summary>是否已与 Broker 建立连接</summary>
        public bool IsConnected => _client?.IsConnected ?? false;

        /// <summary>收到订阅消息时触发，参数为 (主题, 负载文本)</summary>
        public event Action<string, string>? MessageReceived;

        /// <summary>与 Broker 连接成功时触发</summary>
        public event Action? Connected;

        /// <summary>与 Broker 连接断开时触发，参数为断开原因</summary>
        public event Action<string>? Disconnected;

        /// <summary>构造函数 —— 通过依赖注入获取日志服务。</summary>
        public MqttService(ILogService logService)
        {
            _logService = logService;
        }

        #region 连接管理

        /// <summary>
        /// 连接到 MQTT Broker。
        /// </summary>
        public async Task<bool> ConnectAsync(MQTTModel config)
        {
            config.Validate();
            _lastConfig = config;
            _disconnectRequested = false;

            // 释放旧的客户端实例
            if (_client != null)
            {
                try { await _client.DisconnectAsync(); }
                catch { /* 忽略旧连接的断开异常 */ }
                _client.Dispose();
            }

            _client = _factory.CreateMqttClient();

            // 事件回调必须在连接前注册
            _client.ApplicationMessageReceivedAsync += OnApplicationMessageReceivedAsync;
            _client.ConnectedAsync += OnConnectedAsync;
            _client.DisconnectedAsync += OnDisconnectedAsync;

            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId(string.IsNullOrEmpty(config.ClientId)
                    ? Guid.NewGuid().ToString("N")
                    : config.ClientId)
                .WithTcpServer(config.BrokerAddress, config.Port)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(config.KeepAliveSeconds))
                .WithCleanSession(config.CleanSession);

            // 用户名/密码认证（可选）
            if (!string.IsNullOrEmpty(config.Username))
                optionsBuilder.WithCredentials(config.Username, config.Password ?? string.Empty);

            // TLS 加密（可选）。默认校验证书，仅当显式配置 IgnoreCertificateErrors 时跳过
            if (config.UseTls)
            {
                optionsBuilder.WithTlsOptions(o =>
                {
                    if (config.IgnoreCertificateErrors)
                        o.WithCertificateValidationHandler(_ => true);
                });
            }

            // 遗嘱消息（Last Will & Testament，可选）
            if (!string.IsNullOrEmpty(config.WillTopic))
            {
                optionsBuilder.WithWillTopic(config.WillTopic);
                if (config.WillPayload != null)
                    optionsBuilder.WithWillPayload(config.WillPayload);
                optionsBuilder.WithWillQualityOfServiceLevel(ToQos(config.WillQoS));
                if (config.WillRetain)
                    optionsBuilder.WithWillRetain();
            }

            var options = optionsBuilder.Build();

            // 连接（带超时）
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(config.ConnectTimeoutSeconds));

            try
            {
                var result = await _client.ConnectAsync(options, cts.Token);

                if (result.ResultCode == MqttClientConnectResultCode.Success)
                {
                    _logService.Info(
                        $"MQTT 已连接 → {config.BrokerAddress}:{config.Port} (ClientId={options.ClientId})",
                        "MQTT");
                    return true;
                }

                _logService.Error(
                    $"MQTT 连接失败 → {config.BrokerAddress}:{config.Port}, 原因: {result.ResultCode}",
                    "MQTT");
                return false;
            }
            catch (OperationCanceledException)
            {
                _logService.Error(
                    $"MQTT 连接超时 → {config.BrokerAddress}:{config.Port} ({config.ConnectTimeoutSeconds}s)",
                    "MQTT");
                return false;
            }
            catch (Exception ex)
            {
                _logService.Error(
                    $"MQTT 连接异常 → {config.BrokerAddress}:{config.Port}: {ex.Message}",
                    "MQTT",
                    ex);
                return false;
            }
        }

        /// <summary>
        /// 正常断开与 Broker 的连接。会发送 MQTT DISCONNECT 包，Broker 不会触发遗嘱消息。
        /// 设置主动断开标志，避免触发自动重连。
        /// </summary>
        public async Task DisconnectAsync()
        {
            _disconnectRequested = true;

            if (_client == null || !_client.IsConnected)
            {
                _logService.Warn("MQTT 断开请求: 当前未连接", "MQTT");
                return;
            }

            try
            {
                await _client.DisconnectAsync(MqttClientDisconnectOptionsReason.NormalDisconnection);
                _logService.Info("MQTT 已断开连接 (正常)", "MQTT");
            }
            catch (Exception ex)
            {
                _logService.Error($"MQTT 断开异常: {ex.Message}", "MQTT", ex);
            }
        }

        #endregion

        #region 订阅管理

        /// <summary>订阅指定主题的消息。</summary>
        public async Task SubscribeAsync(string topic, int qos = 0)
        {
            if (!EnsureConnected()) return;

            var qosLevel = ToQos(qos);

            var subOptions = _factory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(t => t.WithTopic(topic).WithQualityOfServiceLevel(qosLevel))
                .Build();

            try
            {
                var result = await _client!.SubscribeAsync(subOptions, CancellationToken.None);

                // 检查订阅结果：是否被 Broker 授予 QoS（可能因权限不足被拒绝）
                bool granted = result.Items.Any(i =>
                    i.ResultCode == MqttClientSubscribeResultCode.GrantedQoS0 ||
                    i.ResultCode == MqttClientSubscribeResultCode.GrantedQoS1 ||
                    i.ResultCode == MqttClientSubscribeResultCode.GrantedQoS2);

                if (granted)
                    _logService.Info($"MQTT 订阅成功 → 主题: \"{topic}\", QoS: {qosLevel}", "MQTT");
                else
                    _logService.Warn($"MQTT 订阅被拒绝 → 主题: \"{topic}\", 原因: {result.ReasonString ?? "未知"}", "MQTT");
            }
            catch (Exception ex)
            {
                _logService.Error($"MQTT 订阅失败 → 主题: \"{topic}\": {ex.Message}", "MQTT", ex);
            }
        }

        /// <summary>取消订阅指定主题。</summary>
        public async Task UnsubscribeAsync(string topic)
        {
            if (!EnsureConnected()) return;

            var unsubOptions = _factory.CreateUnsubscribeOptionsBuilder()
                .WithTopicFilter(topic)
                .Build();

            try
            {
                await _client!.UnsubscribeAsync(unsubOptions, CancellationToken.None);
                _logService.Info($"MQTT 取消订阅 → 主题: \"{topic}\"", "MQTT");
            }
            catch (Exception ex)
            {
                _logService.Error($"MQTT 取消订阅失败 → 主题: \"{topic}\": {ex.Message}", "MQTT", ex);
            }
        }

        #endregion

        #region 消息发布

        /// <summary>向指定主题发布消息。</summary>
        public async Task<bool> PublishAsync(string topic, string payload, int qos = 0, bool retain = false)
        {
            if (!EnsureConnected()) return false;

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(ToQos(qos))
                .WithRetainFlag(retain)
                .Build();

            try
            {
                await _client!.PublishAsync(message, CancellationToken.None);
                _logService.Debug(
                    $"MQTT 发布 → 主题: \"{topic}\", QoS: {qos}, Retain: {retain}, 长度: {payload.Length}字符",
                    "MQTT");
                return true;
            }
            catch (Exception ex)
            {
                _logService.Error($"MQTT 发布失败 → 主题: \"{topic}\": {ex.Message}", "MQTT", ex);
                return false;
            }
        }

        #endregion

        #region 断线重连

        /// <summary>
        /// 意外断开后自动重连。
        /// 仅在非主动断开、存在配置、未释放时触发；延迟 5 秒避免频繁抖动。
        /// </summary>
        private async Task ReconnectAsync()
        {
            // 防止多个断开回调并发触发重连
            if (Interlocked.Exchange(ref _reconnectInProgress, 1) == 1) return;

            try
            {
                _logService.Info($"MQTT 连接意外断开，{ReconnectDelay.TotalSeconds:F0} 秒后自动重连...", "MQTT");
                await Task.Delay(ReconnectDelay);

                if (_disposed || _disconnectRequested || _lastConfig == null) return;

                _logService.Info("MQTT 自动重连中...", "MQTT");
                var ok = await ConnectAsync(_lastConfig);
                if (ok)
                    _logService.Info("MQTT 自动重连成功", "MQTT");
                else
                    _logService.Warn("MQTT 自动重连失败，等待下一次断开事件", "MQTT");
            }
            catch (Exception ex)
            {
                _logService.Error($"MQTT 自动重连异常: {ex.Message}", "MQTT", ex);
            }
            finally
            {
                Interlocked.Exchange(ref _reconnectInProgress, 0);
            }
        }

        #endregion

        #region 事件回调（私有）

        /// <summary>
        /// MQTT 消息接收回调 —— 当 Broker 推送已订阅主题的消息时触发。
        /// 将 payload 解析为 UTF-8 文本后通过 MessageReceived 事件向上层通知。
        /// </summary>
        private Task OnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;

            // MQTTnet v5 的 Payload 是 ReadOnlySequence<byte>，
            // BuffersExtensions.ToArray 完整复制多段 buffer，避免 FirstSpan 截断
            var bytes = System.Buffers.BuffersExtensions.ToArray(e.ApplicationMessage.Payload);
            var payload = Encoding.UTF8.GetString(bytes);

            _logService.Debug(
                $"MQTT 收到消息 ← 主题: \"{topic}\", 长度: {payload.Length}字符, QoS: {e.ApplicationMessage.QualityOfServiceLevel}",
                "MQTT");

            MessageReceived?.Invoke(topic, payload);

            return Task.CompletedTask;
        }

        /// <summary>连接成功回调</summary>
        private Task OnConnectedAsync(MqttClientConnectedEventArgs e)
        {
            _logService.Info("MQTT 连接已建立", "MQTT");
            Connected?.Invoke();
            return Task.CompletedTask;
        }

        /// <summary>
        /// 连接断开回调 —— Broker 主动断开或网络异常时触发。
        /// 若是意外断开则自动重连。
        /// </summary>
        private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            var reason = e.Reason.ToString();
            _logService.Warn($"MQTT 连接已断开, 原因: {reason}", "MQTT");
            Disconnected?.Invoke(reason);

            if (!_disconnectRequested && _lastConfig != null && !_disposed)
                _ = ReconnectAsync();

            return Task.CompletedTask;
        }

        #endregion

        #region 辅助方法（私有）

        /// <summary>检查客户端是否已连接，未连接时记录警告并返回 false。</summary>
        private bool EnsureConnected()
        {
            if (_client == null || !_client.IsConnected)
            {
                _logService.Warn("MQTT 操作失败: 客户端未连接", "MQTT");
                return false;
            }
            return true;
        }

        /// <summary>将 int QoS 值转换为 MQTTnet 枚举。</summary>
        private static MqttQualityOfServiceLevel ToQos(int qos) => qos switch
        {
            0 => MqttQualityOfServiceLevel.AtMostOnce,
            1 => MqttQualityOfServiceLevel.AtLeastOnce,
            2 => MqttQualityOfServiceLevel.ExactlyOnce,
            _ => MqttQualityOfServiceLevel.AtMostOnce
        };

        #endregion

        #region IDisposable / IAsyncDisposable

        /// <summary>异步释放 —— 断开连接并释放 MQTT 客户端。推荐应用退出时使用。</summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;

            // 标记主动断开，避免触发重连
            _disconnectRequested = true;

            if (_client != null)
            {
                try
                {
                    if (_client.IsConnected)
                        await _client.DisconnectAsync(MqttClientDisconnectOptionsReason.NormalDisconnection);
                }
                catch { /* 释放时忽略异常 */ }

                _client.Dispose();
                _client = null;
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>同步释放 —— 内部包装异步释放，供 using/退出场景调用。</summary>
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait(TimeSpan.FromSeconds(3));
        }

        #endregion
    }
}
