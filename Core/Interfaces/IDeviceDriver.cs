using Core.Models;

namespace Core.Interfaces
{
    /// <summary>
    /// 设备驱动接口 —— 封装与具体协议设备的一次通信会话。
    /// 每个已连接的设备对应一个驱动实例，由基础设施层实现。
    /// </summary>
    public interface IDeviceDriver : IDisposable
    {
        /// <summary>是否已与物理设备建立连接</summary>
        bool IsConnected { get; }

        /// <summary>根据协议配置建立连接</summary>
        Task<bool> ConnectAsync(IProtocolConfig config);

        /// <summary>断开连接，释放通信资源</summary>
        Task DisconnectAsync();

        /// <summary>执行读取命令，返回解析后的结果</summary>
        Task<DeviceReadResult> ReadAsync(DeviceCommand command);

        /// <summary>执行写入命令</summary>
        Task<DeviceWriteResult> WriteAsync(DeviceCommand command);
    }
}
