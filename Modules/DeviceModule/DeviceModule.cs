using Prism.Ioc;
using Prism.Modularity;
using DeviceModule.Views;
using DeviceModule.ViewModels;

namespace DeviceModule
{
    public class DeviceModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DeviceStateView, DeviceStateViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}