using Core.Interfaces;
using Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace StateMachineModule.ViewModels
{
    /// <summary>队列项在 UI 中的显示包装</summary>
    public class QueueItemDisplay : BindableBase
    {
        private QueueItemStatus _status;
        private string _result = string.Empty;
        private string _time = string.Empty;

        public string Order { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;

        public QueueItemStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string StatusText => Status switch
        {
            QueueItemStatus.Pending => "待执行",
            QueueItemStatus.Running => "执行中",
            QueueItemStatus.Completed => "已完成",
            QueueItemStatus.Failed => "失败",
            QueueItemStatus.Paused => "已暂停",
            QueueItemStatus.Cancelled => "已取消",
            _ => "未知"
        };

        public string Result
        {
            get => _result;
            set => SetProperty(ref _result, value);
        }

        public string Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        public CommandQueueItem Source { get; set; } = null!;
    }

    public class CommandRunViewModel : BindableBase
    {
        private readonly ICommandQueueService _queueService;
        private readonly IDeviceExecutionService _executionService;
        private readonly ILogService _logService;
        private readonly IConfigurationService _configService;

        private bool _isRunning;
        private string _statusText = "就绪";

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (SetProperty(ref _isRunning, value))
                {
                    StartCommand.RaiseCanExecuteChanged();
                    PauseCommand.RaiseCanExecuteChanged();
                    StopCommand.RaiseCanExecuteChanged();
                    ClearCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public ObservableCollection<QueueItemDisplay> QueueItems { get; } = new();

        public DelegateCommand StartCommand { get; }
        public DelegateCommand PauseCommand { get; }
        public DelegateCommand StopCommand { get; }
        public DelegateCommand ClearCommand { get; }

        public CommandRunViewModel(
            ICommandQueueService queueService,
            IDeviceExecutionService executionService,
            ILogService logService,
            IConfigurationService configService)
        {
            _queueService = queueService;
            _executionService = executionService;
            _logService = logService;
            _configService = configService;

            StartCommand = new DelegateCommand(async () => await StartAsync(), () => !IsRunning);
            PauseCommand = new DelegateCommand(() => _queueService.Pause(), () => IsRunning);
            StopCommand = new DelegateCommand(Stop, () => IsRunning);
            ClearCommand = new DelegateCommand(Clear, () => !IsRunning);

            SyncFromQueue();
            _queueService.QueueItems.CollectionChanged += (_, _) => SyncFromQueue();
        }

        private void SyncFromQueue()
        {
            // 保持 UI 与底层队列同步（保留已有显示对象的运行时状态）
            var lookup = QueueItems.ToDictionary(d => d.Source);
            QueueItems.Clear();

            for (int i = 0; i < _queueService.QueueItems.Count; i++)
            {
                var src = _queueService.QueueItems[i];
                QueueItemDisplay display;

                if (lookup.TryGetValue(src, out var existing))
                {
                    display = existing;
                    display.Order = (i + 1).ToString();
                    display.Status = src.Status;
                    display.Result = src.LastResult?.FormattedResult
                        ?? src.LastResult?.ErrorMessage
                        ?? "";
                    display.Time = src.LastExecuted?.ToString("HH:mm:ss")
                        ?? src.EnqueuedTime.ToString("HH:mm:ss");
                }
                else
                {
                    var deviceName = FindDeviceName(src.DeviceId);
                    display = new QueueItemDisplay
                    {
                        Order = (i + 1).ToString(),
                        Name = src.DisplayName,
                        DeviceName = deviceName,
                        DeviceId = src.DeviceId,
                        Status = src.Status,
                        Result = "",
                        Time = src.EnqueuedTime.ToString("HH:mm:ss"),
                        Source = src
                    };
                    // 订阅状态变化
                    src.PropertyChanged += (_, _) => SyncFromQueue();
                }

                QueueItems.Add(display);
            }
        }

        private string FindDeviceName(string deviceId)
        {
            return FindDevice(deviceId)?.Name ?? "未知设备";
        }

        private DeviceModel? FindDevice(string deviceId)
        {
            foreach (var line in _configService.ProductionLines)
                foreach (var dev in line.Devices)
                    if (dev.Id == deviceId)
                        return dev;
            return null;
        }

        private async Task StartAsync()
        {
            IsRunning = true;
            StatusText = "运行中";

            // 执行前自动连接所有涉及到的设备
            var deviceIds = _queueService.QueueItems
                .Where(q => q.Status == QueueItemStatus.Pending)
                .Select(q => q.DeviceId)
                .Distinct();

            foreach (var devId in deviceIds)
            {
                var dev = FindDevice(devId);

                if (dev != null && !_executionService.IsConnected(devId))
                {
                    _logService.Info($"自动连接设备 \"{dev.Name}\"...", "Queue");
                    var ok = await _executionService.ConnectAsync(dev);
                    if (!ok)
                        _logService.Error($"设备 \"{dev.Name}\" 连接失败，相关命令将跳过", "Queue");
                }
            }

            await _queueService.StartAsync(_executionService, _logService);

            IsRunning = false;
            StatusText = "就绪";
            SyncFromQueue();
        }

        private void Stop()
        {
            _queueService.Stop();
            IsRunning = false;
            StatusText = "已停止";
        }

        private void Clear()
        {
            _queueService.Clear();
            QueueItems.Clear();
            StatusText = "就绪";
        }
    }
}
