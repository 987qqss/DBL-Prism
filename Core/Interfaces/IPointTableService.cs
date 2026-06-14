using Core.Models;

namespace Core.Interfaces
{
    public interface IPointTableService
    {
        DevicePoint<float> Temp1 { get; }
        DevicePoint<float> Humidity1 { get; }
        DevicePoint<float> Temp2 { get; }
        DevicePoint<float> Humidity2 { get; }
        DevicePoint<bool> WaterSensor1 { get; }
        DevicePoint<bool> WaterSensor2 { get; }
        DevicePoint<bool> Fuse1 { get; }
        DevicePoint<bool> Fuse2 { get; }
        DevicePoint<bool> Fuse3 { get; }
        DevicePoint<bool> AirConditioner { get; }
        DevicePoint<bool> Dehumidifier1 { get; }
        DevicePoint<bool> Dehumidifier2 { get; }
        DevicePoint<bool> Dehumidifier3 { get; }
        DevicePoint<float> LiquidTemperature { get; }
        DevicePoint<float> LiquidSetCoolTemp { get; }
        DevicePoint<float> LiquidSetHeatTemp { get; }
        DevicePoint<bool> FireSystemState { get; }
        DevicePoint<bool> SmokeAlarm { get; }

        event EventHandler<ModbusReadResult> DataUpdated;
        void Update(ModbusReadResult result);
    }
}