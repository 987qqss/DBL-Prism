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
                    var temp = await _modbusService.ReadInputRegistersAsync(_pointTableService.Temp1.SlaveId, _pointTableService.Temp1.Address, 2);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = _pointTableService.Temp1.SlaveId,
                        StartAddress = _pointTableService.Temp1.Address,
                        Function = ModbusFunctionCode.ReadInputRegisters,
                        RegisterValues = temp,
                    }, ct);

                    var dehumidifierStatus = await _modbusService.ReadCoilsAsync(_pointTableService.Dehumidifier1.SlaveId, _pointTableService.Dehumidifier1.Address, 3);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = _pointTableService.Dehumidifier1.SlaveId,
                        StartAddress = _pointTableService.Dehumidifier1.Address,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = dehumidifierStatus,
                    }, ct);

                    var acStatus = await _modbusService.ReadCoilsAsync(_pointTableService.AirConditioner.SlaveId, _pointTableService.AirConditioner.Address, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = _pointTableService.AirConditioner.SlaveId,
                        StartAddress = _pointTableService.AirConditioner.Address,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = acStatus,
                    }, ct);

                    var liquidCoolingStatus = await _modbusService.ReadInputRegistersAsync(_pointTableService.LiquidTemperature.SlaveId, _pointTableService.LiquidTemperature.Address, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = _pointTableService.LiquidTemperature.SlaveId,
                        StartAddress = _pointTableService.LiquidTemperature.Address,
                        Function = ModbusFunctionCode.ReadInputRegisters,
                        RegisterValues = liquidCoolingStatus,
                    }, ct);

                    var liquidCoolingSetpoint = await _modbusService.ReadHoldingRegistersAsync(_pointTableService.LiquidSetCoolTemp.SlaveId, _pointTableService.LiquidSetCoolTemp.Address, 2);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = _pointTableService.LiquidSetCoolTemp.SlaveId,
                        StartAddress = _pointTableService.LiquidSetCoolTemp.Address,
                        Function = ModbusFunctionCode.ReadHoldingRegisters,
                        RegisterValues = liquidCoolingSetpoint,
                    }, ct);

                    var waterSensorStatus = await _modbusService.ReadCoilsAsync(_pointTableService.WaterSensor1.SlaveId, _pointTableService.WaterSensor1.Address, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = _pointTableService.WaterSensor1.SlaveId,
                        StartAddress = _pointTableService.WaterSensor1.Address,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = waterSensorStatus,
                    }, ct);

                    var waterSensorStatus2 = await _modbusService.ReadCoilsAsync(_pointTableService.WaterSensor2.SlaveId, _pointTableService.WaterSensor2.Address, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = _pointTableService.WaterSensor2.SlaveId,
                        StartAddress = _pointTableService.WaterSensor2.Address,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = waterSensorStatus2,
                    }, ct);

                    var fuseStatus = await _modbusService.ReadCoilsAsync(_pointTableService.Fuse1.SlaveId, _pointTableService.Fuse1.Address, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = _pointTableService.Fuse1.SlaveId,
                        StartAddress = _pointTableService.Fuse1.Address,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = fuseStatus,
                    }, ct);

                    var fuseStatus2 = await _modbusService.ReadCoilsAsync(_pointTableService.Fuse2.SlaveId, _pointTableService.Fuse2.Address, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = _pointTableService.Fuse2.SlaveId,
                        StartAddress = _pointTableService.Fuse2.Address,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = fuseStatus2,
                    }, ct);

                    var fuseStatus3 = await _modbusService.ReadCoilsAsync(_pointTableService.Fuse3.SlaveId, _pointTableService.Fuse3.Address, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = _pointTableService.Fuse3.SlaveId,
                        StartAddress = _pointTableService.Fuse3.Address,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = fuseStatus3,
                    }, ct);

                    var smokeSensorStatus = await _modbusService.ReadCoilsAsync(_pointTableService.SmokeAlarm.SlaveId, _pointTableService.SmokeAlarm.Address, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = _pointTableService.SmokeAlarm.SlaveId,
                        StartAddress = _pointTableService.SmokeAlarm.Address,
                        Function = ModbusFunctionCode.ReadCoils,
                        CoilValues = smokeSensorStatus,
                    }, ct);

                    var fireSensorStatus = await _modbusService.ReadCoilsAsync(_pointTableService.FireSystemState.SlaveId, _pointTableService.FireSystemState.Address, 1);
                    await _readChannel.Writer.WriteAsync(new ModbusReadResult
                    {
                        SlaveId = _pointTableService.FireSystemState.SlaveId,
                        StartAddress = _pointTableService.FireSystemState.Address,
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