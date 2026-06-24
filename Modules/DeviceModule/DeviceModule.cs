using Core.Interfaces;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;
using DeviceModule.Views;
using DeviceModule.ViewModels;
using DeviceModule.Views.Dialog.ProtocolConfig;
using DeviceModule.ViewModels.ProtocolConfig;

namespace DeviceModule
{
    public class DeviceModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<Services.IDialogService, Services.DialogService>();
            containerRegistry.Register<ProductionLineDialogView>();
            containerRegistry.Register<ProductionLineDialogViewModel>();
            containerRegistry.Register<DeviceDialogView>();
            containerRegistry.Register<DeviceDialogViewModel>();
            containerRegistry.Register<CommandDialogView>();
            containerRegistry.Register<CommandDialogViewModel>();

            containerRegistry.Register<ModbusTCPConfigView>();
            containerRegistry.Register<ModbusTCPConfigViewModel>();
            containerRegistry.Register<ModbusRTUConfigView>();
            containerRegistry.Register<ModbusRTUConfigViewModel>();

            containerRegistry.RegisterForNavigation<DeviceStateView, DeviceStateViewModel>();
            containerRegistry.RegisterForNavigation<DeviceCommandTreeView, DeviceCommandTreeViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("DeviceConfigRegion", typeof(DeviceConfigView));
            regionManager.RegisterViewWithRegion("SidebarTreeRegion", typeof(DeviceCommandTreeView));
        }
    }
}