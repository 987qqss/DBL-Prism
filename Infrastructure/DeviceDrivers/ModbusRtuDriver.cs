using Core.Interfaces;
using Core.Models;
using NModbus;
using NModbus.Serial;
using System.IO.Ports;

namespace Infrastructure.DeviceDrivers
{
    /// <summary>Modbus RTU 串口协议驱动，基于 NModbus 库</summary>
    public class ModbusRtuDriver : IDeviceDriver
    {
        private SerialPort? _port;
        private IModbusMaster? _master;
        private ModbusRTUModel? _config;
        private bool _disposed;

        public bool IsConnected => _port?.IsOpen ?? false;

        //这里的连接方法根据传入的配置协议（创建设备类配置协议时创建的配置协议）的参数来连接
        public Task<bool> ConnectAsync(IProtocolConfig config)
        {
            if (config is not ModbusRTUModel rtu)
                throw new ArgumentException("ModbusRtuDriver 需要 ModbusRTUModel");

            _config = rtu;
            _config.Validate();

            try
            {
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

                _port = new SerialPort(rtu.SerialPortName, rtu.BaudRate, parity, rtu.DataBits, stopBits)
                {
                    ReadTimeout = rtu.Timeout,
                    WriteTimeout = rtu.Timeout
                };
                _port.Open();
                _master = new ModbusFactory().CreateRtuMaster(new SerialPortAdapter(_port));
                return Task.FromResult(true);
            }
            catch
            {
                _port?.Dispose();
                _port = null;
                return Task.FromResult(false);
            }
        }

        public Task DisconnectAsync()
        {
            _master?.Dispose();
            _master = null;
            _port?.Close();
            _port?.Dispose();
            _port = null;
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
                    case 0x01:
                        var coils = await _master.ReadCoilsAsync(slaveId, addr, (ushort)len);
                        result.RawValue = coils;
                        result.Success = true;
                        break;

                    case 0x02:
                        var inputs = await _master.ReadInputsAsync(slaveId, addr, (ushort)len);
                        result.RawValue = inputs;
                        result.Success = true;
                        break;

                    case 0x03:
                        var holdingRegs = await _master.ReadHoldingRegistersAsync(slaveId, addr, (ushort)len);
                        result.RawValue = ModbusDataConverter.ConvertRegisters(holdingRegs, command.DataFormat);
                        result.ConvertedValue = ModbusDataConverter.ApplyConversion(result.RawValue, command.Scale, command.Offset);
                        result.Success = true;
                        break;

                    case 0x04:
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
                    case 0x05:
                        await _master.WriteSingleCoilAsync(slaveId, addr, Convert.ToBoolean(command.WriteValue));
                        result.WrittenValue = command.WriteValue;
                        break;

                    case 0x06:
                        var regs = ModbusDataConverter.ValueToRegisters(command.WriteValue, command.DataFormat);
                        await _master.WriteSingleRegisterAsync(slaveId, addr, regs[0]);
                        result.WrittenValue = command.WriteValue;
                        break;

                    case 0x10:
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
            _port?.Close();
            _port?.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
