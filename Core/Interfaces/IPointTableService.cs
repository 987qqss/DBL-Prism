using Core.Models;

namespace Core.Interfaces
{
    public interface IPointTableService
    {
        DataPoint Temp1 { get; }
        DataPoint Humidity1 { get; }
        DataPoint Temp2 { get; }
        DataPoint Humidity2 { get; }
        DataPoint WaterSensor1 { get; }
        DataPoint WaterSensor2 { get; }
        DataPoint Fuse1 { get; }
        DataPoint Fuse2 { get; }
        DataPoint Fuse3 { get; }
        DataPoint AirConditioner { get; }
        DataPoint Dehumidifier1 { get; }
        DataPoint Dehumidifier2 { get; }
        DataPoint Dehumidifier3 { get; }
        DataPoint LiquidTemperature { get; }
        DataPoint LiquidSetCoolTemp { get; }
        DataPoint LiquidSetHeatTemp { get; }
        DataPoint FireSystemState { get; }
        DataPoint SmokeAlarm { get; }

        event EventHandler<ModbusReadResult> DataUpdated;
        void Update(ModbusReadResult result);
    }
}