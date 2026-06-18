using Core.Interfaces;
using Core.Models;

namespace Infrastructure.Services
{
    public class PointTableService : IPointTableService
    {
        public DataPoint Temp1 { get; } = new ModbusDataPoint { Name = "温度1", SlaveId = 1, RegisterAddress = 0, Unit = "°C" };
        public DataPoint Humidity1 { get; } = new ModbusDataPoint { Name = "湿度1", SlaveId = 1, RegisterAddress = 1, Unit = "%" };
        public DataPoint Temp2 { get; } = new ModbusDataPoint { Name = "温度2", SlaveId = 1, RegisterAddress = 2, Unit = "°C" };
        public DataPoint Humidity2 { get; } = new ModbusDataPoint { Name = "湿度2", SlaveId = 1, RegisterAddress = 3, Unit = "%" };
        public DataPoint WaterSensor1 { get; } = new ModbusDataPoint { Name = "水浸传感器1", SlaveId = 1, RegisterAddress = 0 };
        public DataPoint WaterSensor2 { get; } = new ModbusDataPoint { Name = "水浸传感器2", SlaveId = 1, RegisterAddress = 1 };
        public DataPoint Fuse1 { get; } = new ModbusDataPoint { Name = "熔断器1", SlaveId = 1, RegisterAddress = 2 };
        public DataPoint Fuse2 { get; } = new ModbusDataPoint { Name = "熔断器2", SlaveId = 1, RegisterAddress = 3 };
        public DataPoint Fuse3 { get; } = new ModbusDataPoint { Name = "熔断器3", SlaveId = 1, RegisterAddress = 4 };
        public DataPoint AirConditioner { get; } = new ModbusDataPoint { Name = "空调", SlaveId = 1, RegisterAddress = 5 };
        public DataPoint Dehumidifier1 { get; } = new ModbusDataPoint { Name = "除湿机1", SlaveId = 1, RegisterAddress = 6 };
        public DataPoint Dehumidifier2 { get; } = new ModbusDataPoint { Name = "除湿机2", SlaveId = 1, RegisterAddress = 7 };
        public DataPoint Dehumidifier3 { get; } = new ModbusDataPoint { Name = "除湿机3", SlaveId = 1, RegisterAddress = 8 };
        public DataPoint LiquidTemperature { get; } = new ModbusDataPoint { Name = "液冷温度", SlaveId = 1, RegisterAddress = 10, Unit = "°C" };
        public DataPoint LiquidSetCoolTemp { get; } = new ModbusDataPoint { Name = "液冷设定冷却温度", SlaveId = 1, RegisterAddress = 11, Unit = "°C" };
        public DataPoint LiquidSetHeatTemp { get; } = new ModbusDataPoint { Name = "液冷设定加热温度", SlaveId = 1, RegisterAddress = 12, Unit = "°C" };
        public DataPoint FireSystemState { get; } = new ModbusDataPoint { Name = "消防系统状态", SlaveId = 1, RegisterAddress = 9 };
        public DataPoint SmokeAlarm { get; } = new ModbusDataPoint { Name = "烟雾报警", SlaveId = 1, RegisterAddress = 10 };

        public event EventHandler<ModbusReadResult>? DataUpdated;

        public void Update(ModbusReadResult result)
        {
            DataUpdated?.Invoke(this, result);
        }
    }
}