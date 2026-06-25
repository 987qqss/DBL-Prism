using Core.Models;

namespace Core.Interfaces
{
    /// <summary>
    /// 设备执行服务 —— 管理所有设备的驱动生命周期并提供命令执行的统一入口。
    /// ViewModel 通过此接口执行连接/断开/读写操作，不直接接触驱动层。
    /// </summary>
    public interface IDeviceExecutionService
    {
        /// <summary>为指定设备建立连接（根据 device.Config 自动创建对应的协议驱动）</summary>
        Task<bool> ConnectAsync(DeviceModel device);

        /// <summary>断开指定设备的连接并释放驱动资源</summary>
        Task DisconnectAsync(DeviceModel device);

        /// <summary>对已连接的设备执行读取命令</summary>
        Task<DeviceReadResult> ReadAsync(DeviceModel device, DeviceCommand command);

        /// <summary>对已连接的设备执行写入命令</summary>
        Task<DeviceWriteResult> WriteAsync(DeviceModel device, DeviceCommand command);

        /// <summary>查询设备是否已连接</summary>
        bool IsConnected(string deviceId);

        /// <summary>获取设备对应的驱动实例（仅已连接时有效）</summary>
        IDeviceDriver? GetDriver(string deviceId);
    }
}
