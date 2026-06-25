using Core.Interfaces;
using Core.Models;
using Infrastructure.Communication;
using System.IO.Ports;

namespace Infrastructure.DeviceDrivers
{
    /// <summary>Modbus RTU 串口协议驱动</summary>
    public class ModbusRtuDriver : IDeviceDriver
    {
        private readonly ModbusRtuService _modbus;
        private ModbusRTUModel? _config;
        private bool _disposed;

        public bool IsConnected => _modbus.IsConnected;

        public ModbusRtuDriver()
        {
            _modbus = new ModbusRtuService();
        }

        public Task<bool> ConnectAsync(IProtocolConfig config)
        {
            if (config is not ModbusRTUModel rtu)
                throw new ArgumentException($"ModbusRtuDriver 需要 ModbusRTUModel");

            _config = rtu;
            _config.Validate();

            var parity = rtu.Parity switch
            {
                SerialParity.Odd => Parity.Odd,
                SerialParity.Even => Parity.Even,
                SerialParity.Mark => Parity.Mark,
                SerialParity.Space => Parity.Space,
                _ => Parity.None
            };

            var stopBits = rtu.StopBits switch
            {
                SerialStopBits.OnePointFive => StopBits.OnePointFive,
                SerialStopBits.Two => StopBits.Two,
                _ => StopBits.One
            };

            return Task.FromResult(_modbus.Connect(rtu.SerialPortName, rtu.BaudRate, parity, rtu.DataBits, stopBits));
        }

        public Task DisconnectAsync()
        {
            _modbus.Disconnect();
            return Task.CompletedTask;
        }

        public async Task<DeviceReadResult> ReadAsync(DeviceCommand command)
        {
            var result = new DeviceReadResult { CommandName = command.Name };

            try
            {
                if (_config == null)
                    throw new InvalidOperationException("驱动未配置");

                var (fc, addr, len) = ResolveAddress(command);
                var slaveId = _config.SlaveId;
                ushort[] registers;

                switch (fc)
                {
                    case 0x01:
                        var coils = await _modbus.ReadCoilsAsync(slaveId, addr, len);
                        result.RawValue = coils;
                        result.Success = true;
                        break;
                    case 0x02:
                        var inputs = await _modbus.ReadDiscreteInputsAsync(slaveId, addr, len);
                        result.RawValue = inputs;
                        result.Success = true;
                        break;
                    case 0x03:
                        registers = await _modbus.ReadHoldingRegistersAsync(slaveId, addr, len);
                        result.RawValue = ModbusDataConverter.ConvertRegisters(registers, command.DataFormat);
                        result.ConvertedValue = ModbusDataConverter.ApplyConversion(result.RawValue, command.Scale, command.Offset);
                        result.Success = true;
                        break;
                    case 0x04:
                        registers = await _modbus.ReadInputRegistersAsync(slaveId, addr, len);
                        result.RawValue = ModbusDataConverter.ConvertRegisters(registers, command.DataFormat);
                        result.ConvertedValue = ModbusDataConverter.ApplyConversion(result.RawValue, command.Scale, command.Offset);
                        result.Success = true;
                        break;
                    default:
                        throw new NotSupportedException($"不支持的功能码: 0x{fc:X2}");
                }

                result.FormattedValue = ModbusDataConverter.FormatValue(result.RawValue, command.DataFormat, command.Unit);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        public async Task<DeviceWriteResult> WriteAsync(DeviceCommand command)
        {
            var result = new DeviceWriteResult { CommandName = command.Name };

            try
            {
                if (_config == null)
                    throw new InvalidOperationException("驱动未配置");

                var (fc, addr, _) = ResolveAddress(command);
                var slaveId = _config.SlaveId;

                switch (fc)
                {
                    case 0x05:
                        await _modbus.WriteSingleCoilAsync(slaveId, addr, Convert.ToBoolean(command.WriteValue));
                        result.WrittenValue = command.WriteValue;
                        break;
                    case 0x06:
                        var regs = ModbusDataConverter.ValueToRegisters(command.WriteValue, command.DataFormat);
                        await _modbus.WriteSingleRegisterAsync(slaveId, addr, regs[0]);
                        result.WrittenValue = command.WriteValue;
                        break;
                    case 0x10:
                        var multiRegs = ModbusDataConverter.ValueToRegisters(command.WriteValue, command.DataFormat);
                        await _modbus.WriteMultipleRegistersAsync(slaveId, addr, multiRegs);
                        result.WrittenValue = command.WriteValue;
                        break;
                    default:
                        throw new NotSupportedException($"不支持的功能码: 0x{fc:X2}");
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        private static (byte fc, ushort addr, ushort len) ResolveAddress(DeviceCommand cmd)
        {
            if (ModbusDataConverter.TryParseModbusAddress(cmd.ProtocolAddress, out var fc, out var addr, out var len))
                return (fc, addr, len);

            throw new InvalidOperationException($"无法解析 Modbus 协议地址: \"{cmd.ProtocolAddress}\"，期望格式: 功能码:地址:长度 如 03:1000:2");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _modbus.Dispose();
            _disposed = true;
        }
    }
}
