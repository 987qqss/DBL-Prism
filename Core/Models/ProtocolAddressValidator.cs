using Core.Interfaces;

namespace Core.Models
{
    /// <summary>
    /// 协议地址校验器 —— 按协议类型校验 ProtocolAddress 字符串格式。
    /// 在驱动解析前使用，避免格式错误运行时才发现。
    /// </summary>
    public static class ProtocolAddressValidator
    {
        /// <summary>
        /// 校验指定协议的地址格式，非法时抛出 ArgumentException。
        /// </summary>
        public static void Validate(ProtocolType protocolType, string? address)
        {
            switch (protocolType)
            {
                case ProtocolType.ModbusTcp:
                case ProtocolType.ModbusRtu:
                    ValidateModbus(address);
                    break;

                case ProtocolType.S7:
                    ValidateS7(address);
                    break;

                // 其它协议暂无严格格式约定，仅检查非空
                default:
                    if (string.IsNullOrWhiteSpace(address))
                        throw new ArgumentException($"{protocolType} 协议地址不能为空");
                    break;
            }
        }

        /// <summary>校验 Modbus 地址: "功能码:起始地址:长度" 如 "03:1000:2"</summary>
        private static void ValidateModbus(string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Modbus 协议地址不能为空");

            var parts = address.Trim().Split(':');
            if (parts.Length < 2 || parts.Length > 3)
                throw new ArgumentException(
                    $"Modbus 协议地址格式错误: \"{address}\"，期望格式: 功能码:地址:长度 如 03:1000:2");

            if (!byte.TryParse(parts[0], System.Globalization.NumberStyles.HexNumber, null, out _))
                throw new ArgumentException($"功能码必须是十六进制: \"{parts[0]}\"");

            if (!ushort.TryParse(parts[1], System.Globalization.NumberStyles.HexNumber, null, out _))
                throw new ArgumentException($"地址必须是十六进制: \"{parts[1]}\"");

            if (parts.Length == 3 && !ushort.TryParse(parts[2], out _))
                throw new ArgumentException($"长度必须是数字: \"{parts[2]}\"");
        }

        /// <summary>校验 S7 地址: "DB编号:起始偏移:长度" 如 "5:128:32"</summary>
        private static void ValidateS7(string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("S7 协议地址不能为空");

            var parts = address.Trim().Split(':');
            if (parts.Length < 2 || parts.Length > 3)
                throw new ArgumentException(
                    $"S7 协议地址格式错误: \"{address}\"，期望格式: DB编号:偏移:长度 如 5:128:32");

            if (!int.TryParse(parts[0], out _))
                throw new ArgumentException($"DB 编号必须是数字: \"{parts[0]}\"");

            if (!int.TryParse(parts[1], out _))
                throw new ArgumentException($"偏移量必须是数字: \"{parts[1]}\"");

            if (parts.Length == 3 && !int.TryParse(parts[2], out _))
                throw new ArgumentException($"长度必须是数字: \"{parts[2]}\"");
        }
    }
}
