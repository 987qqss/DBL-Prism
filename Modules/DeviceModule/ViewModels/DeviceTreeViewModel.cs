using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Interfaces;
using Core.Models;
using Prism.Events;
using System.Collections.ObjectModel;

namespace DeviceModule.ViewModels
{
    public partial class DeviceTreeViewModel : ObservableObject
    {
        private readonly IEventAggregator _eventAggregator;
        private object? _selectedItem;

        public ObservableCollection<ProductionLineModel> ProductionLines { get; } = new();

        public object? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public bool CanAddDevice => SelectedItem is ProductionLineModel || SelectedItem is DeviceModel;
        public bool CanDeleteProductionLine => SelectedItem is ProductionLineModel;
        public bool CanAddCommand => SelectedItem is DeviceModel;
        public bool CanEditDevice => SelectedItem is DeviceModel;
        public bool CanDeleteDevice => SelectedItem is DeviceModel;
        public bool CanConnect => SelectedItem is DeviceModel device && device.Status != DeviceStatus.Online;
        public bool CanDisconnect => SelectedItem is DeviceModel device && device.Status == DeviceStatus.Online;

        public DeviceTreeViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            LoadMockData();
        }

        private void LoadMockData()
        {
            var productionLine1 = new ProductionLineModel { Id = "PL001", Name = "产线1" };
            
            var device1 = new DeviceModel
            {
                Id = "DEV001",
                Name = "PLC-1",
                Status = DeviceStatus.Online
            };
            device1.SetConfig(new ModbusTCPModel { IpAddress = "192.168.1.100", Port = 502, SlaveId = 1 });
            productionLine1.Devices.Add(device1);

            var device2 = new DeviceModel
            {
                Id = "DEV002",
                Name = "PLC-2",
                Status = DeviceStatus.Online
            };
            device2.SetConfig(new ModbusTCPModel { IpAddress = "192.168.1.101", Port = 502, SlaveId = 1 });
            productionLine1.Devices.Add(device2);

            var productionLine2 = new ProductionLineModel { Id = "PL002", Name = "产线2" };
            
            var device3 = new DeviceModel
            {
                Id = "DEV003",
                Name = "S7-1200",
                Status = DeviceStatus.Offline
            };
            device3.SetConfig(new S7Model { IpAddress = "192.168.1.102", Rack = 0, Slot = 2 });
            productionLine2.Devices.Add(device3);

            var ungrouped = new ProductionLineModel { Id = "PL003", Name = "未分组设备" };
            
            var device4 = new DeviceModel
            {
                Id = "DEV004",
                Name = "Modbus设备-1",
                Status = DeviceStatus.NotConfigured
            };
            device4.SetConfig(new ModbusRTUModel { SerialPortName = "COM3", BaudRate = 9600, SlaveId = 1 });
            ungrouped.Devices.Add(device4);

            ProductionLines.Add(productionLine1);
            ProductionLines.Add(productionLine2);
            ProductionLines.Add(ungrouped);
        }

        [RelayCommand]
        public void AddProductionLine()
        {
            var newLine = new ProductionLineModel { Name = "新产线" };
            ProductionLines.Add(newLine);
            _eventAggregator.GetEvent<SelectedItemChangedEvent>().Publish(newLine);
        }

        [RelayCommand]
        public void AddDevice()
        {
            if (SelectedItem is ProductionLineModel line)
            {
                var newDevice = new DeviceModel
                {
                    Name = "新设备",
                    ProductionLineId = line.Id,
                    ProtocolType = ProtocolType.ModbusTcp
                };
                line.Devices.Add(newDevice);
                _eventAggregator.GetEvent<SelectedItemChangedEvent>().Publish(newDevice);
            }
            else if (SelectedItem is DeviceModel device)
            {
                var prodLine = ProductionLines.FirstOrDefault(l => l.Devices.Contains(device));
                if (prodLine != null)
                {
                    var newDevice = new DeviceModel
                    {
                        Name = "新设备",
                        ProductionLineId = prodLine.Id,
                        ProtocolType = ProtocolType.ModbusTcp
                    };
                    prodLine.Devices.Add(newDevice);
                    _eventAggregator.GetEvent<SelectedItemChangedEvent>().Publish(newDevice);
                }
            }
        }

        [RelayCommand]
        public void DeleteProductionLine()
        {
            if (SelectedItem is ProductionLineModel line)
            {
                ProductionLines.Remove(line);
            }
        }

        [RelayCommand]
        public void EditDevice()
        {
            if (SelectedItem is DeviceModel device)
            {
                _eventAggregator.GetEvent<SelectedItemChangedEvent>().Publish(device);
            }
        }

        [RelayCommand]
        public void DeleteDevice()
        {
            if (SelectedItem is DeviceModel device)
            {
                var line = ProductionLines.FirstOrDefault(l => l.Devices.Contains(device));
                line?.Devices.Remove(device);
            }
        }

        [RelayCommand]
        public void AddCommand()
        {
            if (SelectedItem is DeviceModel device)
            {
                var newCommand = new DeviceCommand
                {
                    Name = "新命令",
                    DeviceId = device.Id,
                    CommandType = CommandType.Read,
                    OperationCode = 0x03 // ReadHoldingRegisters
                };
                device.Commands.Add(newCommand);
            }
        }

        [RelayCommand]
        public void ConnectDevice()
        {
            if (SelectedItem is DeviceModel device)
            {
                device.Status = DeviceStatus.Online;
                OnPropertyChanged(nameof(CanConnect));
                OnPropertyChanged(nameof(CanDisconnect));
            }
        }

        [RelayCommand]
        public void DisconnectDevice()
        {
            if (SelectedItem is DeviceModel device)
            {
                device.Status = DeviceStatus.Offline;
                OnPropertyChanged(nameof(CanConnect));
                OnPropertyChanged(nameof(CanDisconnect));
            }
        }

        [RelayCommand]
        public void OnSelectionChanged(object item)
        {
            SelectedItem = item;
            OnPropertyChanged(nameof(CanAddDevice));
            OnPropertyChanged(nameof(CanDeleteProductionLine));
            OnPropertyChanged(nameof(CanAddCommand));
            OnPropertyChanged(nameof(CanEditDevice));
            OnPropertyChanged(nameof(CanDeleteDevice));
            OnPropertyChanged(nameof(CanConnect));
            OnPropertyChanged(nameof(CanDisconnect));
        }
    }

    public class SelectedItemChangedEvent : PubSubEvent<object> { }
}
