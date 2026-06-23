using CommunityToolkit.Mvvm.ComponentModel;
using Core.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Infrastructure.Services
{
    /// <summary>
    /// 命令执行状态机 —— 管理命令队列的生命周期（入队、执行、暂停、恢复）。
    /// 遵循数据驱动原则：只关心调度，不关心命令的具体内容。
    /// </summary>
    public partial class CommandStateMachine : ObservableObject
    {
        private readonly CommandExecutionService _executionService;
        private readonly Dictionary<string, DeviceModel> _deviceRegistry = new();
        private CancellationTokenSource? _cts;
        private readonly object _lock = new();

        // ─── 状态 ───

        private MachineState _state = MachineState.Idle;
        public MachineState State
        {
            get => _state;
            private set => SetProperty(ref _state, value);
        }

        private int _currentIndex;
        public int CurrentIndex
        {
            get => _currentIndex;
            private set => SetProperty(ref _currentIndex, value);
        }

        private string _statusText = "就绪";
        public string StatusText
        {
            get => _statusText;
            private set => SetProperty(ref _statusText, value);
        }

        // ─── 队列 ───

        public ObservableCollection<CommandQueueItem> Queue { get; } = new();

        // ─── 事件 ───

        public event Action<QueueEventType, CommandQueueItem?, CommandExecutionResult?>? OnQueueEvent;

        // ─── 统计 ───

        private int _totalExecuted;
        public int TotalExecuted
        {
            get => _totalExecuted;
            private set => SetProperty(ref _totalExecuted, value);
        }

        private int _totalFailed;
        public int TotalFailed
        {
            get => _totalFailed;
            private set => SetProperty(ref _totalFailed, value);
        }


        public CommandStateMachine(CommandExecutionService executionService)
        {
            _executionService = executionService;
        }

        // ─── 设备注册 ───

        public void RegisterDevice(DeviceModel device)
        {
            lock (_lock)
            {
                _deviceRegistry[device.Id] = device;
            }
        }

        public void RegisterDevices(IEnumerable<DeviceModel> devices)
        {
            foreach (var d in devices) RegisterDevice(d);
        }

        // ─── 入队 ───

        public CommandQueueItem Enqueue(DeviceCommand command, ScheduleMode schedule = ScheduleMode.Manual)
        {
            var item = CommandQueueItem.FromCommand(command, schedule);
            Queue.Add(item);
            return item;
        }

        public void EnqueueRange(IEnumerable<DeviceCommand> commands, ScheduleMode schedule = ScheduleMode.Sequential)
        {
            foreach (var cmd in commands)
                Enqueue(cmd, schedule);
        }

        public bool Remove(CommandQueueItem item)
        {
            return Queue.Remove(item);
        }

        public void Clear()
        {
            Queue.Clear();
            CurrentIndex = 0;
        }

        // ─── 执行控制 ───

        public async Task StartAsync()
        {
            if (State == MachineState.Running) return;
            if (Queue.Count == 0)
            {
                StatusText = "队列为空，无法启动";
                return;
            }

            _cts = new CancellationTokenSource();
            State = MachineState.Running;
            StatusText = "运行中";
            FireEvent(QueueEventType.QueueStarted, null, null);

            // 按优先级降序排列
            var sorted = Queue.OrderByDescending(q => q.Priority).ToList();

            for (CurrentIndex = 0; CurrentIndex < sorted.Count; CurrentIndex++)
            {
                if (_cts.IsCancellationRequested)
                    break;

                var item = sorted[CurrentIndex];

                // 暂停等待
                while (State == MachineState.Paused && !_cts.IsCancellationRequested)
                    await Task.Delay(100, _cts.Token);

                if (_cts.IsCancellationRequested) break;

                await ExecuteItemAsync(item, _cts.Token);
            }

            if (!_cts.IsCancellationRequested)
            {
                State = MachineState.Completed;
                StatusText = "队列执行完成";
                FireEvent(QueueEventType.QueueCompleted, null, null);
            }
        }

        public void Pause()
        {
            if (State != MachineState.Running) return;
            State = MachineState.Paused;
            StatusText = "已暂停";
            FireEvent(QueueEventType.QueuePaused, null, null);
        }

        public void Resume()
        {
            if (State != MachineState.Paused) return;
            State = MachineState.Running;
            StatusText = "运行中";
            FireEvent(QueueEventType.QueueResumed, null, null);
        }

        public void Stop()
        {
            _cts?.Cancel();
            State = MachineState.Idle;
            StatusText = "已停止";
            CurrentIndex = 0;
            FireEvent(QueueEventType.QueueStopped, null, null);
        }

        // ─── 单条执行 ───

        private async Task ExecuteItemAsync(CommandQueueItem item, CancellationToken ct)
        {
            item.Status = QueueItemStatus.Running;
            FireEvent(QueueEventType.ItemStarted, item, null);

            var stopwatch = Stopwatch.StartNew();
            var device = GetDevice(item.DeviceId);
            var command = item.CommandSnapshot;
            int retries = item.MaxRetries;

            if (device == null || command == null)
            {
                item.Status = QueueItemStatus.Failed;
                item.ErrorCount++;
                TotalFailed++;
                FireEvent(QueueEventType.ItemFailed, item, null);
                return;
            }

            CommandExecutionResult? result = null;
            do
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    result = await _executionService.ExecuteCommandAsync(command, device);
                    result.CommandId = command.Id;
                    result.DeviceId = device.Id;
                    result.Timestamp = DateTime.Now;

                    if (result.Success)
                    {
                        item.Status = QueueItemStatus.Completed;
                        item.LastResult = result;
                        item.LastExecuted = DateTime.Now;
                        item.ExecutionCount++;
                        TotalExecuted++;
                        stopwatch.Stop();
                        result.ExecutionTime = stopwatch.ElapsedMilliseconds;
                        FireEvent(QueueEventType.ItemCompleted, item, result);
                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    result = new CommandExecutionResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        CommandId = command.Id,
                        DeviceId = device.Id,
                        Timestamp = DateTime.Now
                    };
                }

                retries--;
                if (retries >= 0 && !ct.IsCancellationRequested)
                    await Task.Delay(200, ct); // 重试前等待
            }
            while (retries >= 0);

            // 所有重试失败
            item.Status = QueueItemStatus.Failed;
            item.LastResult = result;
            item.LastExecuted = DateTime.Now;
            item.ErrorCount++;
            TotalFailed++;
            stopwatch.Stop();
            if (result != null) result.ExecutionTime = stopwatch.ElapsedMilliseconds;
            FireEvent(QueueEventType.ItemFailed, item, result);
        }

        private DeviceModel? GetDevice(string deviceId)
        {
            lock (_lock)
            {
                return _deviceRegistry.TryGetValue(deviceId, out var device) ? device : null;
            }
        }

        private void FireEvent(QueueEventType type, CommandQueueItem? item, CommandExecutionResult? result)
        {
            OnQueueEvent?.Invoke(type, item, result);
        }
    }
}
