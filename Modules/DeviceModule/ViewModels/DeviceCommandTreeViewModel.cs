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
        private readonly IDeviceExecutionService _executionService;
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

        public DeviceCommandTreeViewModel(IRegionManager regionManager, Services.IDialogService dialogService, ILogService logService, IConfigurationService configService, IDeviceExecutionService executionService)
        {
            _regionManager = regionManager;
            _dialogService = dialogService;
            _logService = logService;
            _configService = configService;
            _executionService = executionService;
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
            //switch (item)
            //{
            //    case DeviceCommand: NavigateTo("OperateView"); break;
            //    case DeviceModel: NavigateTo("DeviceConfigView"); break;
            //}
        }

        private void NavigateTo(string viewName)
        {
            //if (string.IsNullOrWhiteSpace(viewName)) return;
            //_regionManager.RequestNavigate("ContentRegion", viewName);
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
                    _logService.Info($"设备 \"{result.Name}\" 已添加到产线 \"{line.Name}\"", "DeviceTree");
                    SelectedItem = result;
                    _logService.Info($"设备 {result.Name} 添加成功", "DeviceModule");
                }
            }
            catch (Exception ex)
            {
                _logService.Error($"添加设备失败: {ex.Message}", "DeviceTree", ex);
                MessageBox.Show($"添加设备失败: {ex.Message}\n{ex.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void _DeleteProductionLine(ProductionLineModel? line)
        {
            if (line == null) return;
            var name = line.Name;
            ProductionLines.Remove(line);
            _logService.Info($"产线 \"{name}\" 已删除", "DeviceTree");
            SelectedItem = null;
        }

        private void _AddProductionLine()
        {
            var result = _dialogService.ShowProductionLineDialog(null, isEditMode: false);

            if (result != null && !string.IsNullOrWhiteSpace(result.Name))
            {
                ProductionLines.Add(result);
                _logService.Info($"产线 \"{result.Name}\" 添加成功", "DeviceTree");
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
                _logService.Info($"设备 \"{device.Name}\" 协议配置已更新 ({protocolConfig.ProtocolType})", "DeviceTree");
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

        private async void _ConnectDevice(DeviceModel? device)
        {
            if (device == null) return;
            try
            {
                await _executionService.ConnectAsync(device);
            }
            catch (Exception ex)
            {
                _logService.Error($"连接设备 \"{device.Name}\" 失败: {ex.Message}", "DeviceTree", ex);
            }
        }

        private async void _DisconnectDevice(DeviceModel? device)
        {
            if (device == null) return;
            try
            {
                await _executionService.DisconnectAsync(device);
            }
            catch (Exception ex)
            {
                _logService.Error($"断开设备 \"{device.Name}\" 失败: {ex.Message}", "DeviceTree", ex);
            }
        }

        private void _AddCommandToDevice(DeviceModel? device)
        {
            if (device == null) return;

            var result = _dialogService.ShowCommandDialog(null, isEditMode: false);

            if (result != null && !string.IsNullOrWhiteSpace(result.Name))
            {
                result.DeviceId = device.Id;
                device.Commands.Add(result);
                _logService.Info($"命令 \"{result.Name}\" 已添加到设备 \"{device.Name}\"", "DeviceTree");
                SelectedItem = result;
            }
        }

        private void _DeleteDevice(DeviceModel? device)
        {
            if (device == null) return;
            var name = device.Name;
            var line = ProductionLines.FirstOrDefault(l => l.Devices.Contains(device));
            line?.Devices.Remove(device);
            _logService.Info($"设备 \"{name}\" 已删除", "DeviceTree");
            SelectedItem = null;
        }

        #endregion

        #region 命令右键菜单

        private async void _ExecuteCommand(DeviceCommand? cmd)
        {
            if (cmd == null) return;
            SelectedItem = cmd;

            // 查找命令所属的设备
            DeviceModel? device = null;
            foreach (var line in ProductionLines)
                foreach (var dev in line.Devices)
                    if (dev.Commands.Contains(cmd))
                    {
                        device = dev;
                        break;
                    }

            if (device == null)
            {
                _logService.Error($"命令 \"{cmd.Name}\" 未找到所属设备", "DeviceTree");
                return;
            }

            // ─── 统一入口：委托优先 → 数据回退 ───
            if (cmd.ExecuteAction != null)
            {
                // 预定义命令：自动连接，然后传入驱动执行
                if (!_executionService.IsConnected(device.Id))
                {
                    var connected = await _executionService.ConnectAsync(device);
                    if (!connected)
                    {
                        MessageBox.Show($"设备 \"{device.Name}\" 未连接，无法执行命令", "提示",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                var driver = _executionService.GetDriver(device.Id);
                try
                {
                    _logService.Info($"执行预定义命令: \"{cmd.Name}\"", "DeviceTree");
                    await cmd.ExecuteAction(driver);
                }
                catch (Exception ex)
                {
                    _logService.Error($"预定义命令 \"{cmd.Name}\" 执行异常: {ex.Message}", "DeviceTree", ex);
                }
                return;
            }

            // ─── 手动命令：数据驱动执行路径 ───
            if (!_executionService.IsConnected(device.Id))
            {
                _logService.Warn($"执行命令前设备 \"{device.Name}\" 未连接，尝试自动连接...", "DeviceTree");
                var connected = await _executionService.ConnectAsync(device);
                if (!connected)
                {
                    MessageBox.Show($"设备 \"{device.Name}\" 未连接，无法执行命令", "提示",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                if (cmd.CommandType == CommandType.Read || cmd.CommandType == CommandType.ReadWrite)
                {
                    var result = await _executionService.ReadAsync(device, cmd);
                    if (result.Success)
                        _logService.Info($"命令 \"{cmd.Name}\" 执行成功 → {result.FormattedValue}", "DeviceTree");
                    else
                        _logService.Error($"命令 \"{cmd.Name}\" 执行失败: {result.ErrorMessage}", "DeviceTree");
                }

                if (cmd.CommandType == CommandType.Write || cmd.CommandType == CommandType.ReadWrite)
                {
                    var result = await _executionService.WriteAsync(device, cmd);
                    if (result.Success)
                        _logService.Info($"命令 \"{cmd.Name}\" 写入成功", "DeviceTree");
                    else
                        _logService.Error($"命令 \"{cmd.Name}\" 写入失败: {result.ErrorMessage}", "DeviceTree");
                }

                if (cmd.CommandType == CommandType.Custom)
                {
                    _logService.Warn($"自定义命令 \"{cmd.Name}\" 暂不支持", "DeviceTree");
                }
            }
            catch (Exception ex)
            {
                _logService.Error($"执行命令 \"{cmd.Name}\" 异常: {ex.Message}", "DeviceTree", ex);
                MessageBox.Show($"执行命令失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void _DeleteCommand(DeviceCommand? cmd)
        {
            if (cmd == null) return;
            foreach (var line in ProductionLines)
                foreach (var device in line.Devices)
                    if (device.Commands.Contains(cmd))
                    {
                        var cmdName = cmd.Name;
                        var devName = device.Name;
                        device.Commands.Remove(cmd);
                        _logService.Info($"命令 \"{cmdName}\" 已从设备 \"{devName}\" 删除", "DeviceTree");
                        SelectedItem = null;
                        return;
                    }
        }

        #endregion
    }
}