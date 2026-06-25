using CommunityToolkit.Mvvm.ComponentModel;

namespace Core.Models
{
    /// <summary>队列项调度模式</summary>
    public enum ScheduleMode
    {
        Manual,       // 手动触发一次
        Periodic,     // 按周期轮询
        Conditional,  // 条件满足时触发
        Sequential    // 按队列顺序依次执行
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

    /// <summary>
    /// 命令队列项 —— 将一个命令绑定到执行队列中，携带调度策略和运行时状态。
    /// 与 DeviceCommand（数据定义）分离，实现"定义一次，排队多次"。
    /// </summary>
    public partial class CommandQueueItem : ObservableObject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>队列项的显示名称（可自定义，默认取命令名）</summary>
        private string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        // ─── 引用（不复制数据，只引用） ───

        /// <summary>引用的命令 Id → DeviceCommand.Id</summary>
        public string CommandId { get; set; } = string.Empty;

        /// <summary>目标设备 Id → DeviceModel.Id</summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>命令的快照副本（入队时拷贝，防止后续命令定义变更影响队列）</summary>
        public DeviceCommand? CommandSnapshot { get; set; }

        // ─── 调度策略 ───

        /// <summary>调度模式</summary>
        public ScheduleMode Schedule { get; set; } = ScheduleMode.Manual;

        /// <summary>Periodic 模式下的执行间隔（毫秒）</summary>
        public int IntervalMs { get; set; } = 1000;

        /// <summary>Conditional 模式下的触发条件表达式</summary>
        public string? ConditionExpression { get; set; }

        /// <summary>优先级（数值越大优先级越高）</summary>
        public int Priority { get; set; } = 0;

        /// <summary>最大重试次数（0 表示不重试，-1 表示使用命令定义的 RetryCount）</summary>
        public int MaxRetries { get; set; } = 0;

        // ─── 运行时状态 ───

        private QueueItemStatus _status = QueueItemStatus.Pending;
        public QueueItemStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private DateTime? _lastExecuted;
        public DateTime? LastExecuted
        {
            get => _lastExecuted;
            set => SetProperty(ref _lastExecuted, value);
        }

        private CommandExecutionResult? _lastResult;
        public CommandExecutionResult? LastResult
        {
            get => _lastResult;
            set => SetProperty(ref _lastResult, value);
        }

        private int _errorCount;
        public int ErrorCount
        {
            get => _errorCount;
            set => SetProperty(ref _errorCount, value);
        }

        private int _executionCount;
        public int ExecutionCount
        {
            get => _executionCount;
            set => SetProperty(ref _executionCount, value);
        }

        /// <summary>入队时间</summary>
        public DateTime EnqueuedTime { get; set; } = DateTime.Now;

        // ─── 方法 ───

        /// <summary>从命令定义创建快照</summary>
        public static CommandQueueItem FromCommand(DeviceCommand command, ScheduleMode schedule = ScheduleMode.Manual)
        {
            return new CommandQueueItem
            {
                DisplayName = command.Name,
                CommandId = command.Id,
                DeviceId = command.DeviceId,
                CommandSnapshot = command,
                Schedule = schedule,
                MaxRetries = command.RetryCount,
                ConditionExpression = command.TriggerExpression,
                Status = QueueItemStatus.Pending,
                EnqueuedTime = DateTime.Now
            };
        }
    }
}
