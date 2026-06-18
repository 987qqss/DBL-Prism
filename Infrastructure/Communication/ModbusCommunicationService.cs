using Core.Events;
using Core.Interfaces;
using Core.Models;
using Prism.Events;
using System.Collections.Generic;
using System.Threading.Channels;

namespace Infrastructure.Communication
{
    public class ModbusCommunicationService : IModbusCommunicationService
    {
        private readonly IModbusService _modbusService;
        private readonly IPointTableService _pointTableService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Channel<ModbusReadResult> _readChannel;
        private readonly Channel<ModbusWriteCommand> _writeChannel;

        public ModbusCommunicationService(IModbusService modbusService, IPointTableService pointTableService, IEventAggregator eventAggregator)
        {
            _modbusService = modbusService;
            _pointTableService = pointTableService;
            _eventAggregator = eventAggregator;
            _readChannel = Channel.CreateBounded<ModbusReadResult>(100);
            _writeChannel = Channel.CreateBounded<ModbusWriteCommand>(50);
        }

        public void Start(CancellationToken appShutdownToken)
        {
            var readCts = CancellationTokenSource.CreateLinkedTokenSource(appShutdownToken);
            var writeCts = CancellationTokenSource.CreateLinkedTokenSource(appShutdownToken);

            Task.Run(() => ReadConsumer(readCts.Token));
            Task.Run(() => WriteConsumer(writeCts.Token));
        }

        public void Stop()
        {
        }

        public async Task ReadProducer(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var temp1 = (ModbusDataPoint)_pointTableService.Temp1;
                    var temp = await _modbusService.ReadInputRegistersAsync(temp1.SlaveId, temp1.RegisterAddress, 2);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = temp1.SlaveId,
                        StartAddress = temp1.RegisterAddress,
                        Function = ModbusFunctionCode.ReadInputRegisters,
                        RegisterValues = temp,
                    }, ct);

                    var dehumidifier1 = (ModbusDataPoint)_pointTableService.Dehumidifier1;
                    var dehumidifierStatus = await _modbusService.ReadCoilsAsync(dehumidifier1.SlaveId, dehumidifier1.RegisterAddress, 3);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = dehumidifier1.SlaveId,
                        StartAddress = dehumidifier1.RegisterAddress,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = dehumidifierStatus,
                    }, ct);

                    var airConditioner = (ModbusDataPoint)_pointTableService.AirConditioner;
                    var acStatus = await _modbusService.ReadCoilsAsync(airConditioner.SlaveId, airConditioner.RegisterAddress, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = airConditioner.SlaveId,
                        StartAddress = airConditioner.RegisterAddress,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = acStatus,
                    }, ct);

                    var liquidTemp = (ModbusDataPoint)_pointTableService.LiquidTemperature;
                    var liquidCoolingStatus = await _modbusService.ReadInputRegistersAsync(liquidTemp.SlaveId, liquidTemp.RegisterAddress, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = liquidTemp.SlaveId,
                        StartAddress = liquidTemp.RegisterAddress,
                        Function = ModbusFunctionCode.ReadInputRegisters,
                        RegisterValues = liquidCoolingStatus,
                    }, ct);

                    var liquidSetCool = (ModbusDataPoint)_pointTableService.LiquidSetCoolTemp;
                    var liquidCoolingSetpoint = await _modbusService.ReadHoldingRegistersAsync(liquidSetCool.SlaveId, liquidSetCool.RegisterAddress, 2);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = liquidSetCool.SlaveId,
                        StartAddress = liquidSetCool.RegisterAddress,
                        Function = ModbusFunctionCode.ReadHoldingRegisters,
                        RegisterValues = liquidCoolingSetpoint,
                    }, ct);

                    var waterSensor1 = (ModbusDataPoint)_pointTableService.WaterSensor1;
                    var waterSensorStatus = await _modbusService.ReadCoilsAsync(waterSensor1.SlaveId, waterSensor1.RegisterAddress, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = waterSensor1.SlaveId,
                        StartAddress = waterSensor1.RegisterAddress,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = waterSensorStatus,
                    }, ct);

                    var waterSensor2 = (ModbusDataPoint)_pointTableService.WaterSensor2;
                    var waterSensorStatus2 = await _modbusService.ReadCoilsAsync(waterSensor2.SlaveId, waterSensor2.RegisterAddress, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = waterSensor2.SlaveId,
                        StartAddress = waterSensor2.RegisterAddress,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = waterSensorStatus2,
                    }, ct);

                    var fuse1 = (ModbusDataPoint)_pointTableService.Fuse1;
                    var fuseStatus = await _modbusService.ReadCoilsAsync(fuse1.SlaveId, fuse1.RegisterAddress, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = fuse1.SlaveId,
                        StartAddress = fuse1.RegisterAddress,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = fuseStatus,
                    }, ct);

                    var fuse2 = (ModbusDataPoint)_pointTableService.Fuse2;
                    var fuseStatus2 = await _modbusService.ReadCoilsAsync(fuse2.SlaveId, fuse2.RegisterAddress, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = fuse2.SlaveId,
                        StartAddress = fuse2.RegisterAddress,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = fuseStatus2,
                    }, ct);

                    var fuse3 = (ModbusDataPoint)_pointTableService.Fuse3;
                    var fuseStatus3 = await _modbusService.ReadCoilsAsync(fuse3.SlaveId, fuse3.RegisterAddress, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = fuse3.SlaveId,
                        StartAddress = fuse3.RegisterAddress,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = fuseStatus3,
                    }, ct);

                    var smokeAlarm = (ModbusDataPoint)_pointTableService.SmokeAlarm;
                    var smokeSensorStatus = await _modbusService.ReadCoilsAsync(smokeAlarm.SlaveId, smokeAlarm.RegisterAddress, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = smokeAlarm.SlaveId,
                        StartAddress = smokeAlarm.RegisterAddress,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = smokeSensorStatus,
                    }, ct);

                    var fireSystem = (ModbusDataPoint)_pointTableService.FireSystemState;
                    var fireSensorStatus = await _modbusService.ReadCoilsAsync(fireSystem.SlaveId, fireSystem.RegisterAddress, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = fireSystem.SlaveId,
                        StartAddress = fireSystem.RegisterAddress,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = fireSensorStatus,
                    }, ct);

                    await Task.Delay(1000, ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"读取数据时发生错误: {ex.Message}");
                }
            }
        }

        public async Task ReadConsumer(CancellationToken ct)
        {
            await foreach (var result in _readChannel.Reader.ReadAllAsync(ct))
            {
                _eventAggregator.GetEvent<DataUpdatedEvent>().Publish(result);
            }
        }

        public async Task WriteProducer(ModbusWriteCommand command, CancellationToken ct)
        {
            await _writeChannel.Writer.WriteAsync(command, ct);
            var timeoutTask = Task.Delay(5000, ct);
            var completedTask = await Task.WhenAny(command.Completion.Task, timeoutTask);
            if (completedTask == timeoutTask)
                throw new TimeoutException("写入超时");
            await command.Completion.Task;
        }

        public async Task WriteConsumer(CancellationToken ct)
        {
            await foreach (var command in _writeChannel.Reader.ReadAllAsync(ct))
            {
                try
                {
                    switch (command.Function)
                    {
                        case ModbusFunctionCode.ReadCoils:
                            await _modbusService.WriteSingleCoilAsync(command.SlaveId, command.StartAddress, (bool)command.Value!);
                            break;
                        case ModbusFunctionCode.ReadHoldingRegisters:
                            await _modbusService.WriteSingleRegisterAsync(command.SlaveId, command.StartAddress, (ushort)command.Value!);
                            break;
                        default:
                            throw new InvalidOperationException($"不支持写入功能码: {command.Function}");
                    }
                    command.Completion.SetResult(true);
                }
                catch (Exception ex)
                {
                    command.Completion.SetException(ex);
                }
            }
        }
    }
}