using Core.Interfaces;
using Infrastructure.Communication;
using Infrastructure.Services;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Shell.Views;
using System.Windows;
using DeviceModule;

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
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<IPointTableService, PointTableService>();
            containerRegistry.RegisterSingleton<IUserSessionService, UserSessionService>();
            containerRegistry.RegisterSingleton<IModbusCommunicationService, ModbusCommunicationService>();
        }

        protected override void InitializeShell(Window shell)
        {
            base.InitializeShell(shell);
            shell.Show();
        }
    }
}