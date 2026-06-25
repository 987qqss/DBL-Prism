using Core.Interfaces;
using Core.Models;

namespace Infrastructure.DeviceCommands
{
    //这个类是放程序员预定义的设备命令的类，这个类会在程序启动时被反射扫描识别到，
    //并且根据特性标签定义的设备名称来加载到对应的设备的命令集合中去
    /// <summary>
    /// BMS-主控制器 预定义命令集 —— 由 CommandScanner 启动时自动注入。
    /// 每个 [DeviceCommand] 方法 = 设备树中一条可执行命令。
    /// </summary>
    /// 这个类只是一个模板类，当有别的设备需要在程序里添加设备命令的时候就可以仿照这个类来实现
    /// 只要[DeviceCommandClass("BMS-主控制器")]特性构造函数里传入的设备名称与
    /// 设备集合里的某个设备名称一样就可以加载
    [DeviceCommandClass("BMS-主控制器")]//使用特性类标记并且调用一个参数的构造函数传入名称
    public class BmsCommands
    {
        private readonly ILogService _log;

        public BmsCommands(ILogService log)
        {
            _log = log;
        }

        /// <summary>读取总电压</summary>
        [DeviceCommand("读取总电压", "03:1000:2", DataFormat = DataFormat.Int16)]
        public async Task ReadVoltage(IDeviceDriver? driver)//传入一个设备驱动实例，
                                                            //当点击设备连接时就会创建这个实例
        {
            if (driver == null) throw new InvalidOperationException("设备未连接");
            var cmd = new DeviceCommand { ProtocolAddress = "03:1000:2", DataFormat = DataFormat.Int16 };
            var result = await driver.ReadAsync(cmd);
            _log.Info(result.Success
                ? $"📊 总电压: {result.FormattedValue}"
                : $"❌ 读取总电压失败: {result.ErrorMessage}", "BMS");
        }

        /// <summary>读取电芯温度</summary>
        [DeviceCommand("读取电芯温度", "04:1010:4", DataFormat = DataFormat.Int16, Unit = "℃")]
        public async Task ReadCellTemp(IDeviceDriver? driver)
        {
            if (driver == null) throw new InvalidOperationException("设备未连接");
            var cmd = new DeviceCommand { ProtocolAddress = "04:1010:4", DataFormat = DataFormat.Int16 };
            var result = await driver.ReadAsync(cmd);
            _log.Info(result.Success
                ? $"🌡 电芯温度: {result.FormattedValue}"
                : $"❌ 读取电芯温度失败: {result.ErrorMessage}", "BMS");
        }

        /// <summary>设置充电阈值 — 写操作</summary>
        [DeviceCommand("设置充电阈值", "06:2000:1", CommandType = CommandType.Write, DataFormat = DataFormat.UInt16)]
        public async Task SetChargeThreshold(IDeviceDriver? driver)
        {
            if (driver == null) throw new InvalidOperationException("设备未连接");
            var cmd = new DeviceCommand
            {
                ProtocolAddress = "06:2000:1",
                CommandType = CommandType.Write,
                DataFormat = DataFormat.UInt16,
                WriteValue = (ushort)3800  // 默认阈值
            };
            var result = await driver.WriteAsync(cmd);
            _log.Info(result.Success
                ? $"⚡ 充电阈值已设置为 3800mV"
                : $"❌ 设置充电阈值失败: {result.ErrorMessage}", "BMS");
        }

        /// <summary>批量巡检 — 一次读取多个数据点</summary>
        [DeviceCommand("批量巡检", CommandType = CommandType.Custom)]
        public async Task PatrolAll(IDeviceDriver? driver)
        {
            if (driver == null) throw new InvalidOperationException("设备未连接");

            _log.Info("🔍 BMS 批量巡检开始...", "BMS");

            var voltage = await driver.ReadAsync(new DeviceCommand { ProtocolAddress = "03:1000:2", DataFormat = DataFormat.Int16 });
            _log.Info(voltage.Success ? $"  总电压: {voltage.FormattedValue}" : $"  总电压: 读取失败", "BMS");

            var temp = await driver.ReadAsync(new DeviceCommand { ProtocolAddress = "04:1010:4", DataFormat = DataFormat.Int16 });
            _log.Info(temp.Success ? $"  电芯温度: {temp.FormattedValue}" : $"  电芯温度: 读取失败", "BMS");

            _log.Info("🔍 BMS 批量巡检完成", "BMS");
        }
       
    }
}
