using Core.Interfaces;
using Infrastructure.Communication;
using Infrastructure.Services;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;
using Shell.ViewModels;
using Shell.Views;
using Shell.Views.Controls;
using System.Windows;

namespace Shell
{
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainView>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<DeviceModule.DeviceModule>();
            moduleCatalog.AddModule<AlarmModule.AlarmModule>();
            moduleCatalog.AddModule<OperationModule.OperationModule>();
            moduleCatalog.AddModule<DataCollectionModule.DataCollectionModule>();
            moduleCatalog.AddModule<ReportModule.ReportModule>();
            moduleCatalog.AddModule<UserModule.UserModule>();
            moduleCatalog.AddModule<SettingsModule.SettingsModule>();
            moduleCatalog.AddModule<LogModule.LogModule>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<IPointTableService, PointTableService>();
            containerRegistry.RegisterSingleton<IUserSessionService, UserSessionService>();
            containerRegistry.RegisterSingleton<IModbusService, ModbusTcpService>();
            containerRegistry.RegisterSingleton<IModbusCommunicationService, ModbusCommunicationService>();

            containerRegistry.RegisterForNavigation<HomeView>();
            containerRegistry.RegisterForNavigation<CommandRunView, CommandRunViewModel>();
            containerRegistry.RegisterForNavigation<LogView>();
            containerRegistry.RegisterSingleton<MainViewModel>();
            containerRegistry.RegisterSingleton<SidebarViewModel>();
            containerRegistry.RegisterSingleton<MenuBarViewModel>();
        }

        protected override void InitializeShell(Window shell)
        {
            base.InitializeShell(shell);
            shell.Show();

            // 使用 Dispatcher 延迟导航，确保 Region 已注册
            shell.Dispatcher.BeginInvoke(new Action(() =>
            {
                var regionManager = Container.Resolve<IRegionManager>();
                regionManager.RequestNavigate("ContentRegion", "HomeView");
            }));
        }
    }
}
