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
            containerRegistry.RegisterForNavigation<DeviceManagementView, DeviceManagementViewModel>();
            containerRegistry.RegisterForNavigation<DeviceStateView, DeviceStateViewModel>();
            containerRegistry.Register<DeviceWizardViewModel>();
            containerRegistry.Register<DeviceTreeViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}