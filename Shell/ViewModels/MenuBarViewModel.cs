using Core.Interfaces;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using Shell.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace Shell.ViewModels
{
    public class MenuBarViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IConfigurationService _configService;
        private readonly ILogService _logService;
        private string _statusMessage = "系统就绪";
        private bool _isSidebarOpen = false;

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsSidebarOpen
        {
            get => _isSidebarOpen;
            set => SetProperty(ref _isSidebarOpen, value);
        }

        public ObservableCollection<MenuItemModel> FileMenuItems { get; }
        public ObservableCollection<MenuItemModel> EditMenuItems { get; }
        public ObservableCollection<MenuItemModel> ViewMenuItems { get; }
        public ObservableCollection<MenuItemModel> ToolsMenuItems { get; }
        public ObservableCollection<MenuItemModel> UserMenuItems { get; }

        //设置、关于、导航命令
        public DelegateCommand SettingsCommand { get; }
        public DelegateCommand AboutCommand { get; }
        public DelegateCommand ToggleSidebarCommand { get; }

        public DelegateCommand NewProjectCommand { get; }
        public DelegateCommand OpenProjectCommand { get; }
        public DelegateCommand SaveProjectCommand { get; }
        public DelegateCommand ImportConfigCommand { get; }
        public DelegateCommand ExportConfigCommand { get; }
        public DelegateCommand ExitCommand { get; }

        public DelegateCommand UndoCommand { get; }
        public DelegateCommand RedoCommand { get; }
        public DelegateCommand CutCommand { get; }
        public DelegateCommand CopyCommand { get; }
        public DelegateCommand PasteCommand { get; }

        public DelegateCommand<string> NavigateCommand { get; }

        public DelegateCommand StartPollCommand { get; }
        public DelegateCommand StopPollCommand { get; }
        public DelegateCommand ConnectCommand { get; }
        public DelegateCommand DisconnectCommand { get; }
        public DelegateCommand ExportLogCommand { get; }
        public DelegateCommand ClearLogCommand { get; }

        public DelegateCommand LoginCommand { get; }
        public DelegateCommand LogoutCommand { get; }
        public DelegateCommand ChangePasswordCommand { get; }

        public MenuBarViewModel(IRegionManager regionManager, IConfigurationService configService, ILogService logService)
        {
            _regionManager = regionManager;
            _configService = configService;
            _logService = logService;

            SettingsCommand = new DelegateCommand(() => StatusMessage = "打开系统设置...");
            AboutCommand = new DelegateCommand(ShowAbout);

            //弹出导航试图的命令
            ToggleSidebarCommand = new DelegateCommand(() => IsSidebarOpen = !IsSidebarOpen);

            NewProjectCommand = new DelegateCommand(() => StatusMessage = "新建项目");
            OpenProjectCommand = new DelegateCommand(() => StatusMessage = "打开项目");
            SaveProjectCommand = new DelegateCommand(() => StatusMessage = "保存项目");
            ImportConfigCommand = new DelegateCommand(ImportConfig);
            ExportConfigCommand = new DelegateCommand(ExportConfig);
            ExitCommand = new DelegateCommand(() => Application.Current?.Shutdown());

            UndoCommand = new DelegateCommand(() => StatusMessage = "撤销");
            RedoCommand = new DelegateCommand(() => StatusMessage = "重做");
            CutCommand = new DelegateCommand(() => StatusMessage = "剪切");
            CopyCommand = new DelegateCommand(() => StatusMessage = "复制");
            PasteCommand = new DelegateCommand(() => StatusMessage = "粘贴");

            //添加导航命令：根据传入的参数导航到对应的界面
            NavigateCommand = new DelegateCommand<string>(NavigateTo);

            StartPollCommand = new DelegateCommand(() => StatusMessage = "开始轮询");
            StopPollCommand = new DelegateCommand(() => StatusMessage = "停止轮询");
            ConnectCommand = new DelegateCommand(() => StatusMessage = "已连接设备");
            DisconnectCommand = new DelegateCommand(() => StatusMessage = "已断开设备");
            ExportLogCommand = new DelegateCommand(() => StatusMessage = "导出日志");
            ClearLogCommand = new DelegateCommand(() => StatusMessage = "清空日志");

            LoginCommand = new DelegateCommand(() => StatusMessage = "登录");
            LogoutCommand = new DelegateCommand(() => StatusMessage = "注销");
            ChangePasswordCommand = new DelegateCommand(() => StatusMessage = "修改密码");

            FileMenuItems = BuildFileMenu();
            EditMenuItems = BuildEditMenu();
            ViewMenuItems = BuildViewMenu();
            ToolsMenuItems = BuildToolsMenu();
            UserMenuItems = BuildUserMenu();
        }

        //给每个菜单集合添加实例，实例就是一个MenuItemModel类实例，
        //每添加一个实例xmal界面就会根据定义好的模板来显示并且绑定
        private ObservableCollection<MenuItemModel> BuildFileMenu() => new()
        {
            new MenuItemModel { Header = "新建项目", IconKind = "FileDocument", Command = NewProjectCommand, InputGestureText = "Ctrl+N" },
            new MenuItemModel { Header = "打开项目", IconKind = "FolderOpen", Command = OpenProjectCommand, InputGestureText = "Ctrl+O" },
            new MenuItemModel { Header = "保存项目", IconKind = "ContentSave", Command = SaveProjectCommand, InputGestureText = "Ctrl+S" },
            new MenuItemModel { IsSeparator = true },
            new MenuItemModel { Header = "导入配置", IconKind = "Import", Command = ImportConfigCommand },
            new MenuItemModel { Header = "导出配置", IconKind = "Export", Command = ExportConfigCommand },
            new MenuItemModel { IsSeparator = true },
            new MenuItemModel { Header = "退出", IconKind = "ExitToApp", Command = ExitCommand, InputGestureText = "Alt+F4" },
            new MenuItemModel { Header = "退出", IconKind = "ExitToApp", Command = ExitCommand, InputGestureText = "Alt+F4" },
        };

        private ObservableCollection<MenuItemModel> BuildEditMenu() => new()
        {
            new MenuItemModel { Header = "撤销", IconKind = "Undo", Command = UndoCommand, InputGestureText = "Ctrl+Z" },
            new MenuItemModel { Header = "重做", IconKind = "Redo", Command = RedoCommand, InputGestureText = "Ctrl+Y" },
            new MenuItemModel { IsSeparator = true },
            new MenuItemModel { Header = "剪切", IconKind = "ContentCut", Command = CutCommand, InputGestureText = "Ctrl+X" },
            new MenuItemModel { Header = "复制", IconKind = "ContentCopy", Command = CopyCommand, InputGestureText = "Ctrl+C" },
            new MenuItemModel { Header = "粘贴", IconKind = "ContentPaste", Command = PasteCommand, InputGestureText = "Ctrl+V" },
        };

        private ObservableCollection<MenuItemModel> BuildViewMenu() => new()
        {
            new MenuItemModel { Header = "设备信息", IconKind = "Monitor", Command = new DelegateCommand(() => NavigateTo("DeviceStateView")) },
            new MenuItemModel { Header = "报警信息", IconKind = "Alert", Command = new DelegateCommand(() => NavigateTo("AlarmView")) },
            new MenuItemModel { Header = "参数下发", IconKind = "Download", Command = new DelegateCommand(() => NavigateTo("OperateView")) },
            new MenuItemModel { IsSeparator = true },
            new MenuItemModel { Header = "操作日志", IconKind = "History", Command = new DelegateCommand(() => NavigateTo("LogView")) },
            new MenuItemModel { Header = "状态机演示", IconKind = "StateMachine", Command = new DelegateCommand(() => NavigateTo("StateMachineView")) },
        };

        private ObservableCollection<MenuItemModel> BuildToolsMenu() => new()
        {
            new MenuItemModel { Header = "连接设备", IconKind = "Connection", Command = ConnectCommand },
            new MenuItemModel { Header = "断开设备", IconKind = "CloseNetwork", Command = DisconnectCommand },
            new MenuItemModel { IsSeparator = true },
            new MenuItemModel { Header = "启用轮询", IconKind = "Play", Command = StartPollCommand },
            new MenuItemModel { Header = "停止轮询", IconKind = "Stop", Command = StopPollCommand },
            new MenuItemModel { IsSeparator = true },
            new MenuItemModel { Header = "导出日志", IconKind = "FileExport", Command = ExportLogCommand },
            new MenuItemModel { Header = "清空日志", IconKind = "DeleteSweep", Command = ClearLogCommand },
        };

        private ObservableCollection<MenuItemModel> BuildUserMenu() => new()
        {
            new MenuItemModel { Header = "登录", IconKind = "Login", Command = LoginCommand },
            new MenuItemModel { Header = "注销", IconKind = "Logout", Command = LogoutCommand },
            new MenuItemModel { IsSeparator = true },
            new MenuItemModel { Header = "修改密码", IconKind = "KeyChange", Command = ChangePasswordCommand },
        };

        //导航逻辑实现，根据传入的视图名称导航
        private void NavigateTo(string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName)) return;
            StatusMessage = $"正在切换到 [{viewName}]";
            _regionManager.RequestNavigate("ContentRegion", viewName);
        }

        //显示系统信息
        private void ShowAbout()
        {
            StatusMessage = "储能集装箱电池仓数据监测系统 v1.0.0  |  Powered by Prism + MaterialDesign";
            MessageBox.Show(
                "储能集装箱电池仓数据监测系统\n版本: 1.0.0\n框架: Prism 9.0 + MaterialDesign 5.3\n\n© 2026 凯宜利特",
                "关于",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>导出配置：将设备树序列化为 JSON 文件</summary>
        private void ExportConfig()
        {
            var dialog = new SaveFileDialog
            {
                Title = "导出设备树配置",
                Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                FileName = $"DeviceConfig_{DateTime.Now:yyyy-MM-dd_HHmmss}.json",
                DefaultExt = ".json",
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                _configService.ExportConfig(dialog.FileName);
                StatusMessage = $"配置已导出 → {System.IO.Path.GetFileName(dialog.FileName)}";
                _logService.Info($"用户触发导出配置 → {System.IO.Path.GetFileName(dialog.FileName)}", "MenuBar");
            }
            catch (Exception ex)
            {
                StatusMessage = "导出配置失败";
                _logService.Error($"导出配置失败: {ex.Message}", "MenuBar", ex);
                MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>导入配置：从 JSON 文件反序列化设备树</summary>
        private void ImportConfig()
        {
            var dialog = new OpenFileDialog
            {
                Title = "导入设备树配置",
                Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                DefaultExt = ".json",
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                _configService.ImportConfig(dialog.FileName);
                StatusMessage = $"配置已导入 → {System.IO.Path.GetFileName(dialog.FileName)}";
                _logService.Info($"用户触发导入配置 ← {System.IO.Path.GetFileName(dialog.FileName)}", "MenuBar");
            }
            catch (Exception ex)
            {
                StatusMessage = "导入配置失败";
                _logService.Error($"导入配置失败: {ex.Message}", "MenuBar", ex);
                MessageBox.Show($"导入失败: {ex.Message}\n\n请确认文件格式正确", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}