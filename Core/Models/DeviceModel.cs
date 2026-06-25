using CommunityToolkit.Mvvm.ComponentModel;
using Core.Interfaces;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.Models
{
    /// <summary>
    /// 设备模型 —— 表示一个物理/逻辑设备，支持 INotifyPropertyChanged
    /// </summary>
    public partial class DeviceModel : ObservableObject, IDevice
    {
        private string _name = string.Empty;
        private string _productionLineId = string.Empty;
        private DeviceStatus _status = DeviceStatus.NotConfigured;
        private bool _isConnected;
        private ProtocolType _protocolType;

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string ProductionLineId
        {
            get => _productionLineId;
            set => SetProperty(ref _productionLineId, value);
        }

        public ProtocolType ProtocolType
        {
            get => _protocolType;
            set => SetProperty(ref _protocolType, value);
        }

        public IProtocolConfig? Config { get; set; }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public DeviceStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        /// <summary>设备下的命令列表（树形结构叶子节点）</summary>
        public ObservableCollection<DeviceCommand> Commands { get; set; } = new();

        /// <summary>设备下的数据点列表（运行时监控）</summary>
        [JsonIgnore]
        public ObservableCollection<DataPoint> DataPoints { get; } = new();

        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime UpdatedTime { get; set; } = DateTime.Now;

        public Task<bool> Connect()
        {
            Config?.Validate();
            IsConnected = true;
            Status = DeviceStatus.Online;
            return Task.FromResult(true);
        }

        public Task Disconnect()
        {
            IsConnected = false;
            Status = DeviceStatus.Offline;
            return Task.CompletedTask;
        }

        public Task<bool> TestConnection()
        {
            try
            {
                Config?.Validate();
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public T GetConfig<T>() where T : IProtocolConfig
        {
            if (Config is T typedConfig)
                return typedConfig;

            throw new InvalidOperationException(
                $"当前配置类型为 {Config?.GetType().Name ?? "null"}，无法转换为 {typeof(T).Name}");
        }

        public void SetConfig(IProtocolConfig config)
        {
            Config = config;
            ProtocolType = config.ProtocolType;
        }
    }
}
