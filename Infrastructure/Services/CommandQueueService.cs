using Core.Interfaces;
using Core.Models;
using System.Collections.ObjectModel;

namespace Infrastructure.Services
{
    /// <summary>
    /// 命令执行队列服务 —— 维护队列数据与执行状态。
    /// 由 DeviceCommandTreeViewModel（入队）和 CommandRunViewModel（展示/执行）共享。
    /// </summary>
    public class CommandQueueService : ICommandQueueService
    {
        private CancellationTokenSource? _cts;
        private bool _isPaused;

        public ObservableCollection<CommandQueueItem> QueueItems { get; } = new();
        public bool IsRunning { get; private set; }
        public int CurrentIndex { get; private set; } = -1;

        public void Enqueue(DeviceCommand command, string deviceId)
        {
            var item = CommandQueueItem.FromCommand(command);
            item.DeviceId = deviceId;
            QueueItems.Add(item);
        }

        public void Remove(CommandQueueItem item)
        {
            QueueItems.Remove(item);
        }

        public void Clear()
        {
            Stop();
            QueueItems.Clear();
            CurrentIndex = -1;
        }

        /// <summary>启动顺序执行：从当前索引开始，逐个执行队列项</summary>
        public async Task StartAsync(IDeviceExecutionService executionService, ILogService logService)
        {
            if (IsRunning) return;

            _cts = new CancellationTokenSource();
            _isPaused = false;
            IsRunning = true;

            logService.Info($"▶ 命令队列开始执行 ({QueueItems.Count} 项)", "Queue");

            // 如果是新开始，从头执行
            if (CurrentIndex < 0) CurrentIndex = 0;

            for (; CurrentIndex < QueueItems.Count; CurrentIndex++)
            {
                // 检查取消
                if (_cts.IsCancellationRequested)
                {
                    logService.Warn("⏹ 队列执行被停止", "Queue");
                    break;
                }

                // 等待暂停恢复
                while (_isPaused && !_cts.IsCancellationRequested)
                    await Task.Delay(100, _cts.Token);

                if (_cts.IsCancellationRequested) break;

                var item = QueueItems[CurrentIndex];
                if (item.Status == QueueItemStatus.Completed || item.Status == QueueItemStatus.Cancelled)
                    continue;

                item.Status = QueueItemStatus.Running;
                logService.Info($"▶ 执行 [{CurrentIndex + 1}/{QueueItems.Count}] \"{item.DisplayName}\"", "Queue");

                try
                {
                    // 确保对应设备已连接
                    var device = executionService.GetDriver(item.DeviceId);
                    if (device == null)
                    {
                        item.Status = QueueItemStatus.Failed;
                        item.LastResult = new CommandExecutionResult
                        {
                            Success = false,
                            ErrorMessage = "设备未连接",
                            Timestamp = DateTime.Now
                        };
                        logService.Error($"队列项 \"{item.DisplayName}\" 失败: 设备未连接", "Queue");
                        continue;
                    }

                    // 执行读取或写入
                    if (item.CommandSnapshot != null)
                    {
                        if (item.CommandSnapshot.CommandType == CommandType.Read || item.CommandSnapshot.CommandType == CommandType.ReadWrite)
                        {
                            var result = await device.ReadAsync(item.CommandSnapshot);
                            item.LastResult = new CommandExecutionResult
                            {
                                Success = result.Success,
                                FormattedResult = result.FormattedValue,
                                ErrorMessage = result.ErrorMessage,
                                Timestamp = DateTime.Now
                            };
                        }

                        if (item.CommandSnapshot.CommandType == CommandType.Write || item.CommandSnapshot.CommandType == CommandType.ReadWrite)
                        {
                            var result = await device.WriteAsync(item.CommandSnapshot);
                            if (!result.Success && item.LastResult?.Success == true)
                            {
                                item.LastResult = new CommandExecutionResult
                                {
                                    Success = false,
                                    ErrorMessage = result.ErrorMessage,
                                    Timestamp = DateTime.Now
                                };
                            }
                        }
                    }

                    item.Status = item.LastResult?.Success != false ? QueueItemStatus.Completed : QueueItemStatus.Failed;
                    item.LastExecuted = DateTime.Now;
                    item.ExecutionCount++;

                    logService.Info(item.Status == QueueItemStatus.Completed
                        ? $"✅ \"{item.DisplayName}\" 执行成功"
                        : $"❌ \"{item.DisplayName}\" 执行失败: {item.LastResult?.ErrorMessage}", "Queue");
                }
                catch (Exception ex)
                {
                    item.Status = QueueItemStatus.Failed;
                    item.LastResult = new CommandExecutionResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        Timestamp = DateTime.Now
                    };
                    item.ErrorCount++;
                    logService.Error($"队列项 \"{item.DisplayName}\" 异常: {ex.Message}", "Queue", ex);
                }
            }

            IsRunning = false;
            logService.Info("🏁 命令队列执行完成", "Queue");
        }

        public void Pause()
        {
            if (!IsRunning) return;
            _isPaused = true;
        }

        public void Stop()
        {
            _isPaused = false;
            _cts?.Cancel();
            IsRunning = false;
        }
    }
}
