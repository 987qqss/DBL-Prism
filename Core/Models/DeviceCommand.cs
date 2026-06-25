using Core.Interfaces;
using System.Text.Json.Serialization;

namespace Core.Models
{
    /// <summary>
    /// 设备命令 —— 通过 ProtocolAddress 以字符串表达协议地址。
    /// ExecuteAction 委托由 CommandScanner 在启动时为预定义命令注入；
    /// 为 null 时回退到数据驱动执行路径（手动命令）。
    /// </summary>
    public class DeviceCommand
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;

        /// <summary>所属设备 Id</summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>命令类型：Read / Write / ReadWrite / Custom</summary>
        public CommandType CommandType { get; set; } = CommandType.Read;

        /// <summary>
        /// 通用协议地址字符串，格式由各驱动自解释。
        /// Modbus: "功能码:起始地址:数量" 如 "03:1000:2"
        /// S7: "DB编号:起始偏移:长度" 如 "5:128:32"
        /// OPC UA: "ns=2;s=Temperature"
        /// </summary>
        public string ProtocolAddress { get; set; } = string.Empty;

        /// <summary>写入值（CommandType.Write 时使用）</summary>
        public object? WriteValue { get; set; }

        //数据格式
        public DataFormat DataFormat { get; set; } = DataFormat.Int16;

        /// <summary>转换系数：显示值 = 原始值 × Scale + Offset</summary>
        public float Scale { get; set; } = 1.0f;

        /// <summary>偏移量：显示值 = 原始值 × Scale + Offset</summary>
        public float Offset { get; set; } = 0.0f;

        /// <summary>单位标识，如 "V"、"℃"、"%"</summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>失败重试次数</summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>条件触发表达式，如 "Temperature > 45"</summary>
        public string? TriggerExpression { get; set; }

        /// <summary>
        /// 预定义执行委托，签名 Task MethodName(IDeviceDriver? driver)。
        /// 由 CommandScanner 在启动时注入；为 null 时回退到数据驱动路径。
        /// </summary>
        [JsonIgnore]
        public Func<IDeviceDriver?, Task>? ExecuteAction { get; set; }
    }

    // ─── 执行结果 ───

    public class CommandExecutionResult
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FormattedResult { get; set; }
        public long ExecutionTime { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string CommandId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
    }

    // ─── 枚举 ───

    public enum CommandType
    {
        Read,
        Write,
        ReadWrite,
        Custom
    }

    public enum DataFormat
    {
        UInt16,
        Int16,
        UInt32,
        Int32,
        Float,
        Double,
        Bool,
        String,
        ByteArray
    }
}
