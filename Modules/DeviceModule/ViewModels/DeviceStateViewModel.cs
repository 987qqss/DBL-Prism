using CommunityToolkit.Mvvm.ComponentModel;
using Core.Events;
using Core.Interfaces;
using Core.Models;
using Prism.Events;
using System.Collections.ObjectModel;

namespace DeviceModule.ViewModels
{
    /// <summary>命令数据点在 UI 中的显示行</summary>
    public partial class DataPointRow : ObservableObject
    {
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _commandType = string.Empty;
        [ObservableProperty] private string _protocolAddress = string.Empty;
        [ObservableProperty] private string _value = string.Empty;
    }

    public partial class DeviceStateViewModel : ObservableObject
    {
        private readonly IConfigurationService _configService;
        private readonly IEventAggregator _eventAggregator;
        private DeviceModel? _device;

        [ObservableProperty] private string _deviceName = "未选择";
        [ObservableProperty] private string _connectionStatus = "未连接";
        [ObservableProperty] private string _connectionColor = "#EF4444";
        [ObservableProperty] private string _protocolType = "-";
        [ObservableProperty] private string _protocolDetail = "-";
        [ObservableProperty] private string _deviceId = "-";

        /// <summary>数据点列表（XAML 直接绑定）</summary>
        public ObservableCollection<DataPointRow> DataPoints { get; } = new();

        public DeviceStateViewModel(
            IConfigurationService configService,
            IEventAggregator eventAggregator)
        {
            _configService = configService;
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<DeviceSelectedEvent>()
                .Subscribe(OnDeviceSelected, ThreadOption.UIThread);
        }

        private void OnDeviceSelected(DeviceModel device)
        {
            // 从旧设备取消订阅
            if (_device != null)
            {
                _device.PropertyChanged -= OnDevicePropertyChanged;
                _device.Commands.CollectionChanged -= OnCommandsChanged;
            }

            _device = device;

            RefreshDeviceInfo();
            RefreshDataPoints();

            // 订阅新设备的变化
            _device.PropertyChanged += OnDevicePropertyChanged;
            _device.Commands.CollectionChanged += OnCommandsChanged;
        }

        private void OnDevicePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RefreshDeviceInfo();
        }

        private void OnCommandsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RefreshDataPoints();
        }

        private void RefreshDeviceInfo()
        {
            if (_device == null) return;

            DeviceName = _device.Name;
            DeviceId = _device.Id;

            // 连接状态
            ConnectionStatus = _device.Status switch
            {
                DeviceStatus.Online => "已连接",
                DeviceStatus.Offline => "已断开",
                DeviceStatus.Error => "异常",
                _ => "未配置"
            };
            ConnectionColor = _device.Status switch
            {
                DeviceStatus.Online => "#22C55E",
                DeviceStatus.Offline => "#EF4444",
                DeviceStatus.Error => "#EF4444",
                _ => "#F59E0B"
            };

            // 协议类型
            ProtocolType = _device.ProtocolType switch
            {
                Core.Interfaces.ProtocolType.ModbusTcp => "Modbus TCP",
                Core.Interfaces.ProtocolType.ModbusRtu => "Modbus RTU",
                Core.Interfaces.ProtocolType.S7 => "S7 (西门子)",
                Core.Interfaces.ProtocolType.OpcUa => "OPC UA",
                Core.Interfaces.ProtocolType.Dnp3 => "DNP3",
                Core.Interfaces.ProtocolType.Bacnet => "BACnet",
                Core.Interfaces.ProtocolType.Scpi => "SCPI",
                Core.Interfaces.ProtocolType.TcpIp => "TCP/IP 原始",
                Core.Interfaces.ProtocolType.Custom => "自定义",
                _ => "-"
            };

            // 协议详情
            ProtocolDetail = _device.Config switch
            {
                ModbusTCPModel t => $"IP:{t.IpAddress}:{t.Port}  SlaveId:{t.SlaveId}",
                ModbusRTUModel r => $"{r.SerialPortName}  {r.BaudRate},{r.DataBits},{r.Parity},{r.StopBits}  SlaveId:{r.SlaveId}",
                S7Model s => $"IP:{s.IpAddress}  Rack:{s.Rack}  Slot:{s.Slot}",
                _ => _device.Config != null ? _device.Config.ProtocolType.ToString() : "未配置协议参数"
            };
        }

        private void RefreshDataPoints()
        {
            DataPoints.Clear();
            if (_device == null) return;

            foreach (var cmd in _device.Commands)
            {
                DataPoints.Add(new DataPointRow
                {
                    Name = cmd.Name,
                    CommandType = cmd.CommandType switch
                    {
                        CommandType.Read => "读",
                        CommandType.Write => "写",
                        CommandType.ReadWrite => "读写",
                        CommandType.Custom => "自定义",
                        _ => "-"
                    },
                    ProtocolAddress = cmd.ProtocolAddress,
                    Value = cmd.WriteValue?.ToString() ?? "-"
                });
            }
        }
    }
}
