using Core.Interfaces;
using Core.Models;
using Infrastructure.Communication;

namespace Infrastructure.DeviceDrivers
{
    /// <summary>Modbus TCP 协议驱动</summary>
    public class ModbusTcpDriver : IDeviceDriver
    {
        private readonly ModbusTcpService _modbus;
        private ModbusTCPModel? _config;
        private bool _disposed;

        public bool IsConnected => _modbus.IsConnected;

        public ModbusTcpDriver()
        {
            _modbus = new ModbusTcpService();
        }

        public Task<bool> ConnectAsync(IProtocolConfig config)
        {
            if (config is not ModbusTCPModel tcp)
                throw new ArgumentException($"ModbusTcpDriver 需要 ModbusTCPModel");

            _config = tcp;
            _config.Validate();
            return Task.FromResult(_modbus.Connect(tcp.IpAddress, tcp.Port));
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

                // 优先解析 ProtocolAddress，回退旧字段
                var (fc, addr, len) = ResolveAddress(command);
                var slaveId = _config.SlaveId;
                ushort[] registers;

                switch (fc)
                {
                    case 0x01: // Read Coils
                        var coils = await _modbus.ReadCoilsAsync(slaveId, addr, len);
                        result.RawValue = coils;
                        result.Success = true;
                        break;

                    case 0x02: // Read Discrete Inputs
                        var inputs = await _modbus.ReadDiscreteInputsAsync(slaveId, addr, len);
                        result.RawValue = inputs;
                        result.Success = true;
                        break;

                    case 0x03: // Read Holding Registers
                        registers = await _modbus.ReadHoldingRegistersAsync(slaveId, addr, len);
                        result.RawValue = ModbusDataConverter.ConvertRegisters(registers, command.DataFormat);
                        result.ConvertedValue = ModbusDataConverter.ApplyConversion(result.RawValue, command.Scale, command.Offset);
                        result.Success = true;
                        break;

                    case 0x04: // Read Input Registers
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
                    case 0x05: // Write Single Coil
                        await _modbus.WriteSingleCoilAsync(slaveId, addr, Convert.ToBoolean(command.WriteValue));
                        result.WrittenValue = command.WriteValue;
                        break;

                    case 0x06: // Write Single Register
                        var regs = ModbusDataConverter.ValueToRegisters(command.WriteValue, command.DataFormat);
                        await _modbus.WriteSingleRegisterAsync(slaveId, addr, regs[0]);
                        result.WrittenValue = command.WriteValue;
                        break;

                    case 0x10: // Write Multiple Registers
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
