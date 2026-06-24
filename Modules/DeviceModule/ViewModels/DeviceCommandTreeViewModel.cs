using Core.Interfaces;
using Core.Models;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System.Collections.ObjectModel;
using System.Windows;

namespace DeviceModule.ViewModels
{
    public partial class DeviceCommandTreeViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly Services.IDialogService _dialogService;
        private readonly ILogService _logService;
        private object? _selectedItem;

        public ObservableCollection<ProductionLineModel> ProductionLines { get; } = new();

        public object? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                    OnItemSelected(value);
            }
        }

        public DeviceCommandTreeViewModel(IRegionManager regionManager, Services.IDialogService dialogService, ILogService logService)
        {
            _regionManager = regionManager;
            _dialogService = dialogService;
            _logService = logService;
            LoadMockData();
            AddCommand();
            
        }

        public DelegateCommand<ProductionLineModel> AddDeviceToLine { get; private set; } = null!;
        public DelegateCommand<ProductionLineModel> DeleteProductionLine { get; private set; } = null!;

        public DelegateCommand<DeviceModel> EditDevice { get; private set; } = null!;
        public DelegateCommand<DeviceModel> ConnectDevice { get; private set; } = null!;
        public DelegateCommand<DeviceModel> DisconnectDevice { get; private set; } = null!;
        public DelegateCommand<DeviceModel> AddCommandToDevice { get; private set; } = null!;
        public DelegateCommand<DeviceModel> DeleteDevice { get; private set; } = null!;
        public DelegateCommand<DeviceModel> ConfigureProtocol { get; private set; } = null!;

        public DelegateCommand<DeviceCommand> ExecuteCommand { get; private set; } = null!;
        public DelegateCommand<DeviceCommand> DeleteCommand { get; private set; } = null!;
        
        public DelegateCommand AddProductionLine { get; private set; } = null!;

        #region Mock Data

        private void LoadMockData()
        {
            var pl1 = new ProductionLineModel { Id = "PL001", Name = "电池仓产线 A" };
            var dev1 = new DeviceModel { Id = "DEV001", Name = "BMS-主控制器", Status = DeviceStatus.Online };
            dev1.Commands.Add(new DeviceCommand { Id = "CMD001", Name = "读取总电压", CommandType = CommandType.Read, Address = 0x1000, Length = 2 });
            dev1.Commands.Add(new DeviceCommand { Id = "CMD002", Name = "读取电芯温度", CommandType = CommandType.Read, Address = 0x1010, Length = 4 });
            dev1.Commands.Add(new DeviceCommand { Id = "CMD003", Name = "设置充电阈值", CommandType = CommandType.Write, Address = 0x2000, Length = 1 });
            pl1.Devices.Add(dev1);
            var dev2 = new DeviceModel { Id = "DEV002", Name = "PCS-储能变流器", Status = DeviceStatus.Online };
            dev2.Commands.Add(new DeviceCommand { Id = "CMD004", Name = "读取有功功率", CommandType = CommandType.Read, Address = 0x1100, Length = 2 });
            dev2.Commands.Add(new DeviceCommand { Id = "CMD005", Name = "设置功率上限", CommandType = CommandType.Write, Address = 0x2100, Length = 1 });
            dev2.Commands.Add(new DeviceCommand { Id = "CMD006", Name = "读取运行状态", CommandType = CommandType.Read, Address = 0x1102, Length = 1 });
            pl1.Devices.Add(dev2);

            var pl2 = new ProductionLineModel { Id = "PL002", Name = "电池仓产线 B" };
            var dev3 = new DeviceModel { Id = "DEV003", Name = "EMS-能源管理系统", Status = DeviceStatus.Offline };
            dev3.Commands.Add(new DeviceCommand { Id = "CMD007", Name = "读取SOC", CommandType = CommandType.Read, Address = 0x1200, Length = 1 });
            dev3.Commands.Add(new DeviceCommand { Id = "CMD008", Name = "读取SOH", CommandType = CommandType.Read, Address = 0x1201, Length = 1 });
            pl2.Devices.Add(dev3);
            var dev4 = new DeviceModel { Id = "DEV004", Name = "温控系统", Status = DeviceStatus.Online };
            dev4.Commands.Add(new DeviceCommand { Id = "CMD009", Name = "读取环境温度", CommandType = CommandType.Read, Address = 0x1300, Length = 1 });
            dev4.Commands.Add(new DeviceCommand { Id = "CMD010", Name = "读取环境湿度", CommandType = CommandType.Read, Address = 0x1301, Length = 1 });
            dev4.Commands.Add(new DeviceCommand { Id = "CMD011", Name = "设置目标温度", CommandType = CommandType.Write, Address = 0x2200, Length = 1 });
            pl2.Devices.Add(dev4);

            var pl3 = new ProductionLineModel { Id = "PL003", Name = "未分组设备" };
            var dev5 = new DeviceModel { Id = "DEV005", Name = "消防主机", Status = DeviceStatus.NotConfigured };
            dev5.Commands.Add(new DeviceCommand { Id = "CMD012", Name = "读取烟感状态", CommandType = CommandType.Read, Address = 0x1400, Length = 1 });
            pl3.Devices.Add(dev5);

            ProductionLines.Add(pl1);
            ProductionLines.Add(pl2);
            ProductionLines.Add(pl3);
        }

        #endregion

        #region 导航

        private void OnItemSelected(object? item)
        {
            switch (item)
            {
                case DeviceCommand: NavigateTo("OperateView"); break;
                case DeviceModel: NavigateTo("DeviceConfigView"); break;
            }
        }

        private void NavigateTo(string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName)) return;
            _regionManager.RequestNavigate("ContentRegion", viewName);
        }

        #endregion

        public void AddCommand()
        {
            AddDeviceToLine = new DelegateCommand<ProductionLineModel>(_AddDeviceToLine);
            DeleteProductionLine = new DelegateCommand<ProductionLineModel>(_DeleteProductionLine);
            AddProductionLine = new DelegateCommand(_AddProductionLine);

            EditDevice = new DelegateCommand<DeviceModel>(_EditDevice);
            ConnectDevice = new DelegateCommand<DeviceModel>(_ConnectDevice);
            DisconnectDevice = new DelegateCommand<DeviceModel>(_DisconnectDevice);
            AddCommandToDevice = new DelegateCommand<DeviceModel>(_AddCommandToDevice);
            DeleteDevice = new DelegateCommand<DeviceModel>(_DeleteDevice);
            ConfigureProtocol = new DelegateCommand<DeviceModel>(_ConfigureProtocol);

            ExecuteCommand = new DelegateCommand<DeviceCommand>(_ExecuteCommand);
            DeleteCommand = new DelegateCommand<DeviceCommand>(_DeleteCommand);
        }

        #region 产线右键菜单

        private void _AddDeviceToLine(ProductionLineModel? line)
        {
            if (line == null) return;

            try
            {
                var result = _dialogService.ShowDeviceDialog(null, isEditMode: false);

                if (result != null && !string.IsNullOrWhiteSpace(result.Name))
                {
                    result.ProductionLineId = line.Id;
                    line.Devices.Add(result);
                    SelectedItem = result;
                    _logService.Info($"设备 {result.Name} 添加成功", "DeviceModule");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加设备失败: {ex.Message}\n{ex.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
           
        }

        private void _DeleteProductionLine(ProductionLineModel? line)
        {
            if (line == null) return;
            ProductionLines.Remove(line);
            SelectedItem = null;
        }

        private void _AddProductionLine()
        {
            var result = _dialogService.ShowProductionLineDialog(null, isEditMode: false);

            if (result != null && !string.IsNullOrWhiteSpace(result.Name))
            {
                ProductionLines.Add(result);
                SelectedItem = result;
            }
        }

        #endregion

        #region 设备右键菜单

        private void _ConfigureProtocol(DeviceModel? device)
        {
            if (device == null) return;

            var protocolConfig = _dialogService.ShowProtocolConfigDialog(device.ProtocolType, device.Config);
            if (protocolConfig != null)
            {
                device.SetConfig(protocolConfig);
            }
        }

        private void _EditDevice(DeviceModel? device)
        {
            if (device == null) return;

            var result = _dialogService.ShowDeviceDialog(device, isEditMode: true);

            if (result != null)
            {
                RaisePropertyChanged(nameof(ProductionLines));
            }
        }

        private void _ConnectDevice(DeviceModel? device)
        {
            if (device == null) return;
            device.Status = DeviceStatus.Online;
            device.IsConnected = true;
        }

        private void _DisconnectDevice(DeviceModel? device)
        {
            if (device == null) return;
            device.Status = DeviceStatus.Offline;
            device.IsConnected = false;
        }

        private void _AddCommandToDevice(DeviceModel? device)
        {
            if (device == null) return;

            var result = _dialogService.ShowCommandDialog(null, isEditMode: false);

            if (result != null && !string.IsNullOrWhiteSpace(result.Name))
            {
                result.DeviceId = device.Id;
                device.Commands.Add(result);
                SelectedItem = result;
            }
        }

        private void _DeleteDevice(DeviceModel? device)
        {
            if (device == null) return;
            var line = ProductionLines.FirstOrDefault(l => l.Devices.Contains(device));
            line?.Devices.Remove(device);
            SelectedItem = null;
        }

        #endregion

        #region 命令右键菜单

        private void _ExecuteCommand(DeviceCommand? cmd)
        {
            if (cmd == null) return;
            SelectedItem = cmd;
            NavigateTo("OperateView");
        }

        private void _DeleteCommand(DeviceCommand? cmd)
        {
            if (cmd == null) return;
            foreach (var line in ProductionLines)
                foreach (var device in line.Devices)
                    if (device.Commands.Contains(cmd))
                    {
                        device.Commands.Remove(cmd);
                        SelectedItem = null;
                        return;
                    }
        }

        #endregion
    }
}