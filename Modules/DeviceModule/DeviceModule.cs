using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;
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
            containerRegistry.RegisterForNavigation<DeviceConfigView, DeviceConfigViewModel>();
            containerRegistry.Register<DeviceWizardViewModel>();
            containerRegistry.Register<DeviceTreeViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("DeviceConfigRegion", typeof(DeviceConfigView));
        }
    }
}