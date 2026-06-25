using Core.Interfaces;
using Core.Models;
using Infrastructure.Communication;

namespace Infrastructure.DeviceDrivers
{
    /// <summary>S7 (西门子) 协议驱动。
    /// ProtocolAddress 格式: "DB编号:起始偏移:读取长度" 如 "5:128:32"</summary>
    public class S7Driver : IDeviceDriver
    {
        private readonly S7Service _s7;
        private S7Model? _config;
        private bool _disposed;

        public bool IsConnected => _s7.IsConnected;

        public S7Driver()
        {
            _s7 = new S7Service();
        }

        public Task<bool> ConnectAsync(IProtocolConfig config)
        {
            if (config is not S7Model s7)
                throw new ArgumentException($"S7Driver 需要 S7Model");

            _config = s7;
            _config.Validate();
            return Task.FromResult(_s7.Connect(s7.IpAddress, s7.Rack, s7.Slot));
        }

        public Task DisconnectAsync()
        {
            _s7.Disconnect();
            return Task.CompletedTask;
        }

        public async Task<DeviceReadResult> ReadAsync(DeviceCommand command)
        {
            var result = new DeviceReadResult { CommandName = command.Name };

            try
            {
                var (dbNumber, startOffset, length) = ResolveAddress(command);

                var rawBytes = await _s7.ReadDataAsync(dbNumber, startOffset, length);

                result.RawValue = command.DataFormat switch
                {
                    DataFormat.UInt16 => rawBytes.Length >= 2 ? BitConverter.ToUInt16(SwapBytes(rawBytes), 0) : (object)(ushort)0,
                    DataFormat.Int16 => rawBytes.Length >= 2 ? BitConverter.ToInt16(SwapBytes(rawBytes), 0) : (short)0,
                    DataFormat.UInt32 => rawBytes.Length >= 4 ? BitConverter.ToUInt32(SwapBytes(rawBytes), 0) : (uint)0,
                    DataFormat.Int32 => rawBytes.Length >= 4 ? BitConverter.ToInt32(SwapBytes(rawBytes), 0) : 0,
                    DataFormat.Float => rawBytes.Length >= 4 ? BitConverter.ToSingle(SwapBytes(rawBytes), 0) : 0f,
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

                await _s7.WriteDataAsync(dbNumber, startOffset, data);
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

            throw new InvalidOperationException($"无法解析 S7 协议地址: \"{cmd.ProtocolAddress}\"，期望格式: DB编号:偏移:长度 如 5:128:32");
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
            _s7.Dispose();
            _disposed = true;
        }
    }
}
