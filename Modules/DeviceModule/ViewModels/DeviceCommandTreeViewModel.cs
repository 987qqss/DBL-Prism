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
        private readonly IConfigurationService _configService;
        private object? _selectedItem;

        public object? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                    OnItemSelected(value);
            }
        }

        public DeviceCommandTreeViewModel(IRegionManager regionManager, Services.IDialogService dialogService, ILogService logService, IConfigurationService configService)
        {
            _regionManager = regionManager;
            _dialogService = dialogService;
            _logService = logService;
            _configService = configService;
            AddCommand();
        }
        /// <summary>绑定到配置服务的全局唯一产线集合</summary>
        public ObservableCollection<ProductionLineModel> ProductionLines => _configService.ProductionLines;
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