using System.Collections.ObjectModel;

namespace Core.Models
{
    /// <summary>
    /// 设备命令 —— 描述对一个数据点的读写操作定义（数据驱动，非代码方法）
    /// 可用于 UI 命令树展示，也可被加入 CommandQueueItem 进入执行队列
    /// </summary>
    public class DeviceCommand
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        /// <summary>所属设备 Id</summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>关联的数据点 Id（可选）</summary>
        public string? DataPointId { get; set; }

        /// <summary>命令分类（如 "电气参数" / "温度参数" / "状态参数"），便于 UI 分组</summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>命令类型：Read / Write / ReadWrite / Custom</summary>
        public CommandType CommandType { get; set; } = CommandType.Read;

        /// <summary>协议操作码（Modbus 功能码 0x03 等）</summary>
        public byte OperationCode { get; set; } = 0x00;

        /// <summary>寄存器地址</summary>
        public ushort Address { get; set; } = 0;
        /// <summary>寄存器数量</summary>
        public ushort Length { get; set; } = 1;

        /// <summary>写入值（CommandType.Write 时使用）</summary>
        public object? WriteValue { get; set; }

        public DataFormat DataFormat { get; set; } = DataFormat.Int16;
        public float Scale { get; set; } = 1.0f;
        public float Offset { get; set; } = 0.0f;
        /// <summary>单位标识，如 "V"、"℃"、"%"</summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>自定义协议的原始请求数据</summary>
        public string RequestData { get; set; } = string.Empty;

        /// <summary>是否为系统内置命令</summary>
        public bool IsSystemCommand { get; set; } = false;

        // ─── 执行控制字段（新增）───

        /// <summary>本条命令超时时间（毫秒），0 表示使用默认</summary>
        public int Timeout { get; set; } = 0;

        /// <summary>本条命令失败重试次数</summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>执行失败时是否触发报警</summary>
        public bool IsCritical { get; set; } = false;

        /// <summary>条件触发表达式（如 "Temperature > 45"），非空时仅当条件满足才执行</summary>
        public string? TriggerExpression { get; set; }

        /// <summary>扩展参数集合</summary>
        public ObservableCollection<CommandParameter> Parameters { get; } = new();
    }

    // ─── 执行结果 ───

    public class CommandExecutionResult
    {
        public bool Success { get; set; }
        public object? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FormattedResult { get; set; }
        public long ExecutionTime { get; set; }

        /// <summary>执行时间戳</summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>关联的命令 Id</summary>
        public string CommandId { get; set; } = string.Empty;

        /// <summary>关联的设备 Id</summary>
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

    // ─── 队列相关枚举 ───

    /// <summary>队列项调度模式</summary>
    public enum ScheduleMode
    {
        /// <summary>手动触发一次</summary>
        Manual,
        /// <summary>按周期轮询</summary>
        Periodic,
        /// <summary>条件满足时触发</summary>
        Conditional,
        /// <summary>按队列顺序依次执行</summary>
        Sequential
    }

    /// <summary>队列项运行时状态</summary>
    public enum QueueItemStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Paused,
        Cancelled
    }

    /// <summary>状态机状态</summary>
    public enum MachineState
    {
        Idle,
        Running,
        Paused,
        Error,
        Completed
    }

    /// <summary>队列事件类型（用于 UI 通知）</summary>
    public enum QueueEventType
    {
        ItemStarted,
        ItemCompleted,
        ItemFailed,
        ItemSkipped,
        QueueStarted,
        QueuePaused,
        QueueResumed,
        QueueStopped,
        QueueCompleted
    }

    // ─── 命令参数 ───

    public class CommandParameter
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
