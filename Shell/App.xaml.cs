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
            moduleCatalog.AddModule<StateMachineModule.StateMachineModule>();
            moduleCatalog.AddModule<ReportModule.ReportModule>();
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

            // 命令执行队列服务（设备命令树 → 入队，状态机面板 → 展示/执行）
            containerRegistry.RegisterSingleton<ICommandQueueService, CommandQueueService>();

            // MQTT 服务（应用层通信，事件推送源）
            containerRegistry.RegisterSingleton<IMqttService, MqttService>();

            // ─── 数据监测架构：数据中心 + 订阅者 ───
            // 数据中心（Broker）：生产者写入，订阅者消费
            containerRegistry.RegisterSingleton<IDataPointStore, DataPointStore>();
            // 数据点配置服务（独立监测点配置）
            containerRegistry.RegisterSingleton<IDataPointConfigService, DataPointConfigService>();
            // 采集服务（生产者）：轮询/订阅 → 写入数据中心
            containerRegistry.RegisterSingleton<DataCollectionService>();
            // 报警引擎（订阅者）：状态机判定
            containerRegistry.RegisterSingleton<IAlarmEngine, AlarmEngine>();

            containerRegistry.RegisterForNavigation<HomeView,HomeViewModel>();
            //containerRegistry.RegisterForNavigation<CommandRunView, CommandRunViewModel>();
            //containerRegistry.RegisterForNavigation<LogView>();
            containerRegistry.RegisterSingleton<MainViewModel>();
            containerRegistry.RegisterSingleton<MenuBarViewModel>();
        }

        protected override void InitializeShell(Window shell)
        {
            base.InitializeShell(shell);

            // 启动数据监测架构：加载数据点配置 → 启动采集 → 启动报警
            StartMonitoring();

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

        /// <summary>
        /// 启动数据监测架构：
        /// 1. 加载数据点配置（独立监测点）
        /// 2. 启动采集服务（生产者：轮询/订阅 → 写入 DataPointStore）
        /// 3. 启动报警引擎（订阅者：状态机判定）
        /// </summary>
        private void StartMonitoring()
        {
            try
            {
                // 1. 加载数据点配置
                var configService = Container.Resolve<IDataPointConfigService>();
                configService.Load();

                // 2. 启动采集服务（生产者）
                var collectionService = Container.Resolve<DataCollectionService>();
                collectionService.Start();

                // 3. 启动报警引擎（订阅者）
                var alarmEngine = Container.Resolve<IAlarmEngine>();
                alarmEngine.Start();
            }
            catch (Exception ex)
            {
                var logService = Container.Resolve<ILogService>();
                logService.Error($"启动数据监测架构失败: {ex.Message}", "App", ex);
            }
        }
    }
}
