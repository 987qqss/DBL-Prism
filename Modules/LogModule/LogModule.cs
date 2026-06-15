using Prism.Ioc;
using Prism.Modularity;
using LogModule.Views;
using LogModule.ViewModels;

namespace LogModule
{
    public class LogModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<LogView, LogViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}