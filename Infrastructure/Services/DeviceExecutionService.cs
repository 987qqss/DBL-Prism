using Core.Interfaces;
using Core.Models;
using Infrastructure.DeviceDrivers;

namespace Infrastructure.Services
{
    /// <summary>
    /// 设备执行服务 —— 维护已连接设备的驱动实例池，
    /// 根据协议类型创建对应驱动，统一管理连接/断开/命令执行生命周期。
    /// </summary>
    public class DeviceExecutionService : IDeviceExecutionService
    {
        private readonly Dictionary<string, IDeviceDriver> _drivers = new();
        private readonly ILogService _logService;
        private readonly object _lock = new();

        public DeviceExecutionService(ILogService logService)
        {
            _logService = logService;
        }

        /// <summary>为设备创建并连接协议驱动</summary>
        public async Task<bool> ConnectAsync(DeviceModel device)
        {
            if (device.Config == null)
            {
                _logService.Warn($"设备 \"{device.Name}\" 未配置协议，无法连接", "Driver");
                return false;
            }

            // 已连接则直接返回
            lock (_lock)
            {
                if (_drivers.ContainsKey(device.Id))
                {
                    _logService.Info($"设备 \"{device.Name}\" 已处于连接状态", "Driver");
                    return true;
                }
            }

            IDeviceDriver driver;
            try
            {
                driver = CreateDriver(device.Config.ProtocolType);
                var ok = await driver.ConnectAsync(device.Config);

                if (!ok)
                {
                    _logService.Error($"设备 \"{device.Name}\" 连接失败（{device.Config.ProtocolType}）", "Driver");
                    driver.Dispose();
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logService.Error($"设备 \"{device.Name}\" 驱动创建或连接异常: {ex.Message}", "Driver", ex);
                return false;
            }

            lock (_lock)
            {
                // 双重检查：并发连接同一个设备时，后到者复用先到者的连接
                if (_drivers.TryGetValue(device.Id, out var existing))
                {
                    driver.Dispose(); // 释放自己刚创建的
                    driver = existing;
                }
                else
                {
                    _drivers[device.Id] = driver;
                }
            }

            device.IsConnected = true;
            device.Status = DeviceStatus.Online;
            _logService.Info($"设备 \"{device.Name}\" 连接成功 → {device.Config.ProtocolType}", "Driver");
            return true;
        }

        /// <summary>断开设备连接并释放驱动资源</summary>
        public async Task DisconnectAsync(DeviceModel device)
        {
            IDeviceDriver? driver;
            lock (_lock)
            {
                if (!_drivers.Remove(device.Id, out driver))
                {
                    _logService.Warn($"设备 \"{device.Name}\" 未处于连接状态", "Driver");
                    return;
                }
            }

            try { await driver.DisconnectAsync(); } catch { /* 忽略断开异常 */ }
            driver.Dispose();

            device.IsConnected = false;
            device.Status = DeviceStatus.Offline;
            _logService.Info($"设备 \"{device.Name}\" 已断开连接", "Driver");
        }

        /// <summary>执行读取命令</summary>
        public async Task<DeviceReadResult> ReadAsync(DeviceModel device, DeviceCommand command)
        {
            var driver = GetDriverOrThrow(device);
            _logService.Info($"执行读取: 设备 \"{device.Name}\" → 命令 \"{command.Name}\"", "Driver");
            var result = await driver.ReadAsync(command);
            if (result.Success)
                _logService.Info($"读取成功: {result.FormattedValue}", "Driver");
            else
                _logService.Error($"读取失败: {result.ErrorMessage}", "Driver");
            return result;
        }

        /// <summary>执行写入命令</summary>
        public async Task<DeviceWriteResult> WriteAsync(DeviceModel device, DeviceCommand command)
        {
            var driver = GetDriverOrThrow(device);
            _logService.Info($"执行写入: 设备 \"{device.Name}\" → 命令 \"{command.Name}\"", "Driver");
            var result = await driver.WriteAsync(command);
            if (result.Success)
                _logService.Info($"写入成功", "Driver");
            else
                _logService.Error($"写入失败: {result.ErrorMessage}", "Driver");
            return result;
        }

        public bool IsConnected(string deviceId)
        {
            lock (_lock) return _drivers.ContainsKey(deviceId);
        }

        public IDeviceDriver? GetDriver(string deviceId)
        {
            lock (_lock) return _drivers.TryGetValue(deviceId, out var d) ? d : null;
        }

        /// <summary>应用退出时断开所有设备</summary>
        public async Task DisconnectAllAsync()
        {
            List<IDeviceDriver> drivers;
            lock (_lock)
            {
                drivers = _drivers.Values.ToList();
                _drivers.Clear();
            }

            foreach (var d in drivers)
            {
                try { await d.DisconnectAsync(); } catch { }
                d.Dispose();
            }
        }

        private IDeviceDriver GetDriverOrThrow(DeviceModel device)
        {
            lock (_lock)
            {
                if (_drivers.TryGetValue(device.Id, out var driver))
                    return driver;
            }
            throw new InvalidOperationException($"设备 \"{device.Name}\" 未连接，请先建立连接后执行命令");
        }

        /// <summary>协议类型 → 驱动实例的工厂映射</summary>
        private static IDeviceDriver CreateDriver(ProtocolType protocolType) => protocolType switch
        {
            ProtocolType.ModbusTcp => new ModbusTcpDriver(),
            ProtocolType.ModbusRtu => new ModbusRtuDriver(),
            ProtocolType.S7 => new S7Driver(),
            ProtocolType.TcpIp => throw new NotSupportedException("TCP/IP 原始协议驱动尚未实现"),
            ProtocolType.OpcUa => throw new NotSupportedException("OPC UA 协议驱动尚未实现"),
            ProtocolType.Dnp3 => throw new NotSupportedException("DNP3 协议驱动尚未实现"),
            ProtocolType.Bacnet => throw new NotSupportedException("BACnet 协议驱动尚未实现"),
            ProtocolType.Scpi => throw new NotSupportedException("SCPI 协议驱动尚未实现"),
            ProtocolType.Custom => throw new NotSupportedException("自定义协议驱动尚未实现"),
            _ => throw new ArgumentOutOfRangeException(nameof(protocolType), $"未知协议: {protocolType}")
        };
    }
}
