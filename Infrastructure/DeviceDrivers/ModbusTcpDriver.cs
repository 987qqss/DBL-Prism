using Core.Interfaces;
using Core.Models;
using NModbus;
using System.Net.Sockets;

namespace Infrastructure.DeviceDrivers
{
    /// <summary>Modbus TCP 协议驱动，基于 NModbus 库</summary>
    public class ModbusTcpDriver : IDeviceDriver
    {
        private TcpClient? _client;
        private IModbusMaster? _master;
        private ModbusTCPModel? _config;
        private bool _disposed;

        public bool IsConnected => _client?.Connected ?? false;

        public Task<bool> ConnectAsync(IProtocolConfig config)
        {
            if (config is not ModbusTCPModel tcp)
                throw new ArgumentException($"ModbusTcpDriver 需要 ModbusTCPModel");

            _config = tcp;
            _config.Validate();

            try
            {
                _client = new TcpClient();
                if (!_client.ConnectAsync(tcp.IpAddress, tcp.Port).Wait(TimeSpan.FromSeconds(3)))
                {
                    _client.Dispose();
                    _client = null;
                    return Task.FromResult(false);
                }

                _master = new ModbusFactory().CreateMaster(_client);
                return Task.FromResult(true);
            }
            catch
            {
                _client?.Dispose();
                _client = null;
                return Task.FromResult(false);
            }
        }

        public Task DisconnectAsync()
        {
            _master?.Dispose();
            _master = null;
            _client?.Close();
            _client = null;
            return Task.CompletedTask;
        }

        public async Task<DeviceReadResult> ReadAsync(DeviceCommand command)
        {
            var result = new DeviceReadResult { CommandName = command.Name };

            try
            {
                if (_config == null || _master == null)
                    throw new InvalidOperationException("驱动未配置或未连接");

                var (fc, addr, len) = ResolveAddress(command);
                var slaveId = _config.SlaveId;

                switch (fc)
                {
                    case 0x01: // Read Coils
                        var coils = await _master.ReadCoilsAsync(slaveId, addr, (ushort)len);
                        result.RawValue = coils;
                        result.Success = true;
                        break;

                    case 0x02: // Read Discrete Inputs
                        var inputs = await _master.ReadInputsAsync(slaveId, addr, (ushort)len);
                        result.RawValue = inputs;
                        result.Success = true;
                        break;

                    case 0x03: // Read Holding Registers
                        var holdingRegs = await _master.ReadHoldingRegistersAsync(slaveId, addr, (ushort)len);
                        result.RawValue = ModbusDataConverter.ConvertRegisters(holdingRegs, command.DataFormat);
                        result.ConvertedValue = ModbusDataConverter.ApplyConversion(result.RawValue, command.Scale, command.Offset);
                        result.Success = true;
                        break;

                    case 0x04: // Read Input Registers
                        var inputRegs = await _master.ReadInputRegistersAsync(slaveId, addr, (ushort)len);
                        result.RawValue = ModbusDataConverter.ConvertRegisters(inputRegs, command.DataFormat);
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
                if (_config == null || _master == null)
                    throw new InvalidOperationException("驱动未配置或未连接");

                var (fc, addr, _) = ResolveAddress(command);
                var slaveId = _config.SlaveId;

                switch (fc)
                {
                    case 0x05: // Write Single Coil
                        await _master.WriteSingleCoilAsync(slaveId, addr, Convert.ToBoolean(command.WriteValue));
                        result.WrittenValue = command.WriteValue;
                        break;

                    case 0x06: // Write Single Register
                        var regs = ModbusDataConverter.ValueToRegisters(command.WriteValue, command.DataFormat);
                        await _master.WriteSingleRegisterAsync(slaveId, addr, regs[0]);
                        result.WrittenValue = command.WriteValue;
                        break;

                    case 0x10: // Write Multiple Registers
                        var multiRegs = ModbusDataConverter.ValueToRegisters(command.WriteValue, command.DataFormat);
                        await _master.WriteMultipleRegistersAsync(slaveId, addr, multiRegs);
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

            throw new InvalidOperationException(
                $"无法解析 Modbus 协议地址: \"{cmd.ProtocolAddress}\"，期望格式: 功能码:地址:长度 如 03:1000:2");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _master?.Dispose();
            _client?.Close();
            _client?.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
