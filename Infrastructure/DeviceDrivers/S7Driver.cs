using Core.Interfaces;
using Core.Models;
using S7.Net;

namespace Infrastructure.DeviceDrivers
{
    /// <summary>S7 (西门子) 协议驱动，基于 S7.Net 库。
    /// ProtocolAddress 格式: "DB编号:起始偏移:读取长度" 如 "5:128:32"</summary>
    public class S7Driver : IDeviceDriver
    {
        private Plc? _plc;
        private S7Model? _config;
        private bool _disposed;

        public bool IsConnected => _plc?.IsConnected ?? false;

        public Task<bool> ConnectAsync(IProtocolConfig config)
        {
            if (config is not S7Model s7)
                throw new ArgumentException("S7Driver 需要 S7Model");

            _config = s7;
            _config.Validate();

            try
            {
                _plc = new Plc(CpuType.S71200, s7.IpAddress, (short)s7.Rack, (short)s7.Slot);
                _plc.Open();
                return Task.FromResult(_plc.IsConnected);
            }
            catch
            {
                _plc?.Close();
                _plc = null;
                return Task.FromResult(false);
            }
        }

        public Task DisconnectAsync()
        {
            _plc?.Close();
            _plc = null;
            return Task.CompletedTask;
        }

        public async Task<DeviceReadResult> ReadAsync(DeviceCommand command)
        {
            var result = new DeviceReadResult { CommandName = command.Name };

            try
            {
                if (_config == null || _plc == null || !_plc.IsConnected)
                    throw new InvalidOperationException("驱动未配置或未连接");

                var (dbNumber, startOffset, length) = ResolveAddress(command);

                var rawBytes = await Task.Run(() =>
                    _plc.ReadBytes(DataType.DataBlock, dbNumber, startOffset, length));

                result.RawValue = command.DataFormat switch
                {
                    DataFormat.UInt16 => rawBytes.Length >= 2
                        ? BitConverter.ToUInt16(SwapBytes(rawBytes), 0)
                        : (object)(ushort)0,
                    DataFormat.Int16 => rawBytes.Length >= 2
                        ? BitConverter.ToInt16(SwapBytes(rawBytes), 0)
                        : (short)0,
                    DataFormat.UInt32 => rawBytes.Length >= 4
                        ? BitConverter.ToUInt32(SwapBytes(rawBytes), 0)
                        : (uint)0,
                    DataFormat.Int32 => rawBytes.Length >= 4
                        ? BitConverter.ToInt32(SwapBytes(rawBytes), 0)
                        : 0,
                    DataFormat.Float => rawBytes.Length >= 4
                        ? BitConverter.ToSingle(SwapBytes(rawBytes), 0)
                        : 0f,
                    DataFormat.String => System.Text.Encoding.ASCII.GetString(rawBytes).TrimEnd('\0'),
                    DataFormat.ByteArray => rawBytes,
                    _ => rawBytes
                };

                result.Success = true;
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
                if (_config == null || _plc == null || !_plc.IsConnected)
                    throw new InvalidOperationException("驱动未配置或未连接");

                var (dbNumber, startOffset, _) = ResolveAddress(command);

                byte[] data = command.DataFormat switch
                {
                    DataFormat.UInt16 => SwapBytes(BitConverter.GetBytes(Convert.ToUInt16(command.WriteValue))),
                    DataFormat.Int16 => SwapBytes(BitConverter.GetBytes(Convert.ToInt16(command.WriteValue))),
                    DataFormat.UInt32 => SwapBytes(BitConverter.GetBytes(Convert.ToUInt32(command.WriteValue))),
                    DataFormat.Int32 => SwapBytes(BitConverter.GetBytes(Convert.ToInt32(command.WriteValue))),
                    DataFormat.Float => SwapBytes(BitConverter.GetBytes(Convert.ToSingle(command.WriteValue))),
                    DataFormat.String => System.Text.Encoding.ASCII.GetBytes(command.WriteValue?.ToString() ?? ""),
                    DataFormat.ByteArray => command.WriteValue as byte[] ?? Array.Empty<byte>(),
                    _ => throw new NotSupportedException($"S7 写入不支持 {command.DataFormat}")
                };

                await Task.Run(() =>
                    _plc.WriteBytes(DataType.DataBlock, dbNumber, startOffset, data));

                result.WrittenValue = command.WriteValue;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        private static (int dbNumber, int offset, int length) ResolveAddress(DeviceCommand cmd)
        {
            if (ModbusDataConverter.TryParseS7Address(cmd.ProtocolAddress, out var db, out var off, out var len))
                return (db, off, len);

            throw new InvalidOperationException(
                $"无法解析 S7 协议地址: \"{cmd.ProtocolAddress}\"，期望格式: DB编号:偏移:长度 如 5:128:32");
        }

        private static byte[] SwapBytes(byte[] input)
        {
            var swapped = new byte[input.Length];
            for (int i = 0; i < input.Length; i += 2)
            {
                if (i + 1 < input.Length)
                {
                    swapped[i] = input[i + 1];
                    swapped[i + 1] = input[i];
                }
                else swapped[i] = input[i];
            }
            return swapped;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _plc?.Close();
            _plc = null;
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
