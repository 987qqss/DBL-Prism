using Core.Interfaces;
using DeviceModule.Services;
using Infrastructure.Services;
using LogModule.Services;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;
using Shell.ViewModels;
using Shell.Views;
using System.Windows;

namespace Shell
{
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            var logService = Container.Resolve<ILogService>();
            logService.Info("=== 储能集装箱电池仓数据监测系统 启动 ===", "System");
            logService.Info($"启动时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", "System");
            return Container.Resolve<MainView>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<LogModule.LogModule>();
            moduleCatalog.AddModule<DeviceModule.DeviceModule>();
            moduleCatalog.AddModule<AlarmModule.AlarmModule>();
            moduleCatalog.AddModule<OperationModule.OperationModule>();
            moduleCatalog.AddModule<StateMachineModule.StateMachineModule>();
            moduleCatalog.AddModule<ReportModule.ReportModule>();
            moduleCatalog.AddModule<UserModule.UserModule>();
            moduleCatalog.AddModule<SettingsModule.SettingsModule>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<IUserSessionService, UserSessionService>();

            // 日志服务（必须在 ConfigurationService 之前注册，因为后者依赖 ILogService）
            containerRegistry.RegisterSingleton<ILogService, LogService>();

            // 命令扫描器（启动时反射扫描预定义命令类）
            containerRegistry.RegisterSingleton<CommandScanner>();

            // 配置服务（必须在 CreateShell 前注册，MenuBarViewModel 依赖它）
            containerRegistry.RegisterSingleton<IConfigurationService, ConfigurationService>();

            // 设备执行服务（管理驱动生命周期，支持连接/断开/命令执行）
            containerRegistry.RegisterSingleton<IDeviceExecutionService, DeviceExecutionService>();

            containerRegistry.RegisterForNavigation<HomeView,HomeViewModel>();
            //containerRegistry.RegisterForNavigation<CommandRunView, CommandRunViewModel>();
            //containerRegistry.RegisterForNavigation<LogView>();
            containerRegistry.RegisterSingleton<MainViewModel>();
            containerRegistry.RegisterSingleton<MenuBarViewModel>();
        }

        protected override void InitializeShell(Window shell)
        {
            base.InitializeShell(shell);
            shell.Show();

            var logService = Container.Resolve<ILogService>();
            logService.Info("主窗口初始化完成", "Shell");

            // 使用 Dispatcher 延迟导航，确保 Region 已注册
            shell.Dispatcher.BeginInvoke(new Action(() =>
            {
                var regionManager = Container.Resolve<IRegionManager>();
                regionManager.RequestNavigate("ContentRegion", "HomeView");
                logService.Info("导航至首页 HomeView", "Shell");
            }));
        }
    }
}
