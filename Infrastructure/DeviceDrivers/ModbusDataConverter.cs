using Core.Models;

namespace Infrastructure.DeviceDrivers
{
    /// <summary>Modbus 寄存器与 .NET 类型的转换工具</summary>
    internal static class ModbusDataConverter
    {
        /// <summary>
        /// 尝试解析协议地址字符串，成功时通过 out 参数返回各字段。
        /// Modbus 格式: "功能码:起始地址:数量" 如 "03:1000:2"
        /// </summary>
        public static bool TryParseModbusAddress(string protocolAddress,
            out byte functionCode, out ushort address, out ushort length)
        {
            functionCode = 0;
            address = 0;
            length = 1;

            if (string.IsNullOrWhiteSpace(protocolAddress))
                return false;

            var parts = protocolAddress.Trim().Split(':');
            if (parts.Length < 2 || parts.Length > 3)
                return false;

            if (!byte.TryParse(parts[0], System.Globalization.NumberStyles.HexNumber, null, out functionCode))
                return false;

            if (!ushort.TryParse(parts[1], System.Globalization.NumberStyles.HexNumber, null, out address))
                return false;

            if (parts.Length >= 3)
                ushort.TryParse(parts[2], out length);

            if (length == 0) length = 1;
            return true;
        }

        /// <summary>
        /// 尝试解析 S7 协议地址字符串。
        /// S7 格式: "DB编号:起始偏移:长度" 如 "5:128:32"
        /// </summary>
        public static bool TryParseS7Address(string protocolAddress,
            out int dbNumber, out int offset, out int length)
        {
            dbNumber = 0;
            offset = 0;
            length = 1;

            if (string.IsNullOrWhiteSpace(protocolAddress))
                return false;

            var parts = protocolAddress.Trim().Split(':');
            if (parts.Length < 2 || parts.Length > 3)
                return false;

            if (!int.TryParse(parts[0], out dbNumber))
                return false;

            if (!int.TryParse(parts[1], out offset))
                return false;

            if (parts.Length >= 3)
                int.TryParse(parts[2], out length);

            if (length == 0) length = 1;
            return true;
        }

        /// <summary>将 Modbus 寄存器数组转换为对应的 C# 值</summary>
        public static object ConvertRegisters(ushort[] registers, DataFormat format)
        {
            return format switch
            {
                DataFormat.UInt16 => registers[0],
                DataFormat.Int16 => unchecked((short)registers[0]),
                DataFormat.UInt32 => (uint)(registers[0] << 16 | registers[1]),
                DataFormat.Int32 => (int)(registers[0] << 16 | registers[1]),
                DataFormat.Float => BitConverter.ToSingle(RegistersToBytes(registers, 4), 0),
                DataFormat.Double => BitConverter.ToDouble(RegistersToBytes(registers, 8), 0),
                DataFormat.Bool => (registers[0] & 0x0001) != 0,
                DataFormat.String => System.Text.Encoding.ASCII.GetString(RegistersToBytes(registers, registers.Length * 2)).TrimEnd('\0'),
                DataFormat.ByteArray => RegistersToBytes(registers, registers.Length * 2),
                _ => registers[0]
            };
        }

        /// <summary>应用线性换算: converted = rawValue * Scale + Offset</summary>
        public static double ApplyConversion(object rawValue, float scale, float offset)
        {
            var raw = rawValue switch
            {
                ushort v => (double)v,
                short v => v,
                uint v => v,
                int v => v,
                float v => v,
                double v => v,
                bool v => v ? 1.0 : 0.0,
                _ => 0.0
            };
            return raw * scale + offset;
        }

        /// <summary>将 C# 值转换为 Modbus 寄存器数组，用于写入</summary>
        public static ushort[] ValueToRegisters(object? value, DataFormat format)
        {
            switch (format)
            {
                case DataFormat.UInt16:
                    return new[] { Convert.ToUInt16(value) };
                case DataFormat.Int16:
                    return new[] { unchecked((ushort)Convert.ToInt16(value)) };
                case DataFormat.UInt32:
                    var u32 = Convert.ToUInt32(value);
                    return new[] { (ushort)(u32 >> 16), (ushort)(u32 & 0xFFFF) };
                case DataFormat.Int32:
                    var i32 = Convert.ToInt32(value);
                    return new[] { (ushort)(i32 >> 16), (ushort)(i32 & 0xFFFF) };
                case DataFormat.Float:
                    var fBytes = BitConverter.GetBytes(Convert.ToSingle(value));
                    return new[] { (ushort)(fBytes[0] << 8 | fBytes[1]), (ushort)(fBytes[2] << 8 | fBytes[3]) };
                case DataFormat.Bool:
                    return new[] { Convert.ToBoolean(value) ? (ushort)0xFF00 : (ushort)0x0000 };
                default:
                    throw new NotSupportedException($"不支持将 {format} 格式转换为寄存器");
            }
        }

        public static string FormatValue(object? rawValue, DataFormat format, string unit)
        {
            var suffix = string.IsNullOrEmpty(unit) ? "" : $" {unit}";
            return format switch
            {
                DataFormat.Bool => Convert.ToBoolean(rawValue) ? "ON" : "OFF",
                DataFormat.String => rawValue?.ToString() ?? "",
                DataFormat.ByteArray => rawValue is byte[] b ? BitConverter.ToString(b) : "",
                _ => $"{rawValue}{suffix}"
            };
        }

        private static byte[] RegistersToBytes(ushort[] registers, int byteCount)
        {
            var bytes = new byte[byteCount];
            for (int i = 0; i < registers.Length && i * 2 + 1 < byteCount; i++)
            {
                bytes[i * 2] = (byte)(registers[i] >> 8);
                bytes[i * 2 + 1] = (byte)(registers[i] & 0xFF);
            }
            return bytes;
        }
    }
}
