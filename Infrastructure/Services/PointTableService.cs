using Core.Interfaces;
using Core.Models;

namespace Infrastructure.Services
{
    public class PointTableService : IPointTableService
    {
        public DevicePoint<float> Temp1 { get; } = new DevicePoint<float> { Name = "温度1", SlaveId = 1, Address = 0, Unit = "°C" };
        public DevicePoint<float> Humidity1 { get; } = new DevicePoint<float> { Name = "湿度1", SlaveId = 1, Address = 1, Unit = "%" };
        public DevicePoint<float> Temp2 { get; } = new DevicePoint<float> { Name = "温度2", SlaveId = 1, Address = 2, Unit = "°C" };
        public DevicePoint<float> Humidity2 { get; } = new DevicePoint<float> { Name = "湿度2", SlaveId = 1, Address = 3, Unit = "%" };
        public DevicePoint<bool> WaterSensor1 { get; } = new DevicePoint<bool> { Name = "水浸传感器1", SlaveId = 1, Address = 0 };
        public DevicePoint<bool> WaterSensor2 { get; } = new DevicePoint<bool> { Name = "水浸传感器2", SlaveId = 1, Address = 1 };
        public DevicePoint<bool> Fuse1 { get; } = new DevicePoint<bool> { Name = "熔断器1", SlaveId = 1, Address = 2 };
        public DevicePoint<bool> Fuse2 { get; } = new DevicePoint<bool> { Name = "熔断器2", SlaveId = 1, Address = 3 };
        public DevicePoint<bool> Fuse3 { get; } = new DevicePoint<bool> { Name = "熔断器3", SlaveId = 1, Address = 4 };
        public DevicePoint<bool> AirConditioner { get; } = new DevicePoint<bool> { Name = "空调", SlaveId = 1, Address = 5 };
        public DevicePoint<bool> Dehumidifier1 { get; } = new DevicePoint<bool> { Name = "除湿机1", SlaveId = 1, Address = 6 };
        public DevicePoint<bool> Dehumidifier2 { get; } = new DevicePoint<bool> { Name = "除湿机2", SlaveId = 1, Address = 7 };
        public DevicePoint<bool> Dehumidifier3 { get; } = new DevicePoint<bool> { Name = "除湿机3", SlaveId = 1, Address = 8 };
        public DevicePoint<float> LiquidTemperature { get; } = new DevicePoint<float> { Name = "液冷温度", SlaveId = 1, Address = 10, Unit = "°C" };
        public DevicePoint<float> LiquidSetCoolTemp { get; } = new DevicePoint<float> { Name = "液冷设定冷却温度", SlaveId = 1, Address = 11, Unit = "°C" };
        public DevicePoint<float> LiquidSetHeatTemp { get; } = new DevicePoint<float> { Name = "液冷设定加热温度", SlaveId = 1, Address = 12, Unit = "°C" };
        public DevicePoint<bool> FireSystemState { get; } = new DevicePoint<bool> { Name = "消防系统状态", SlaveId = 1, Address = 9 };
        public DevicePoint<bool> SmokeAlarm { get; } = new DevicePoint<bool> { Name = "烟雾报警", SlaveId = 1, Address = 10 };

        public event EventHandler<ModbusReadResult>? DataUpdated;

        public void Update(ModbusReadResult result)
        {
            DataUpdated?.Invoke(this, result);
        }
    }
}