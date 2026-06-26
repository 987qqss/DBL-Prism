using Core.Models;
using System.Collections.ObjectModel;

namespace Core.Interfaces
{
    /// <summary>
    /// 命令执行队列服务 —— DeviceCommandTreeViewModel 和 CommandRunViewModel 共享的队列数据源。
    /// 右键"添加到执行队列" → 入队；状态机面板 → 显示并顺序执行。
    /// </summary>
    public interface ICommandQueueService
    {
        /// <summary>队列项集合（UI 直接绑定）</summary>
        ObservableCollection<CommandQueueItem> QueueItems { get; }

        /// <summary>队列运行状态</summary>
        bool IsRunning { get; }

        /// <summary>当前执行到的索引</summary>
        int CurrentIndex { get; }

        /// <summary>入队一个命令</summary>
        void Enqueue(DeviceCommand command, string deviceId);

        /// <summary>移除指定队列项</summary>
        void Remove(CommandQueueItem item);

        /// <summary>清空队列</summary>
        void Clear();

        /// <summary>启动顺序执行（由状态机面板调用）</summary>
        Task StartAsync(IDeviceExecutionService executionService, ILogService logService);

        /// <summary>暂停执行</summary>
        void Pause();

        /// <summary>停止执行并重置</summary>
        void Stop();
    }
}
