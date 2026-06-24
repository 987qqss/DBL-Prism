using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;
using LogModule.Views;
using LogModule.ViewModels;
using LogModule.Services;
using Core.Interfaces;
using NLog;

namespace LogModule
{
    public class LogModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            LogManager.LoadConfiguration("NLog.config");
            containerRegistry.RegisterSingleton<ILogService, LogService>();
            containerRegistry.RegisterForNavigation<LogView, LogViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("LogRegion", typeof(LogView));
        }
    }
}
