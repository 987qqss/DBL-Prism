using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;
using DeviceModule.Views;
using DeviceModule.ViewModels;
using DeviceModule.Services;

namespace DeviceModule
{
    public class DeviceModule : IModule
    {
        //这个方式是在模块加载时调用，复制注册模块内部的类到全局容器中
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册对话框服务和对话框视图
            containerRegistry.Register<Services.IDialogService, Services.DialogService>();
            containerRegistry.Register<ProductionLineDialogView>();
            containerRegistry.Register<ProductionLineDialogViewModel>();
            containerRegistry.Register<DeviceDialogView>();
            containerRegistry.Register<DeviceDialogViewModel>();
            containerRegistry.Register<CommandDialogView>();
            containerRegistry.Register<CommandDialogViewModel>();

            // 注册模块视图
            containerRegistry.RegisterForNavigation<DeviceManagementView, DeviceManagementViewModel>();
            containerRegistry.RegisterForNavigation<DeviceStateView, DeviceStateViewModel>();
            containerRegistry.RegisterForNavigation<DeviceTreeView, DeviceTreeViewModel>();
            containerRegistry.RegisterForNavigation<DeviceCommandTreeView, DeviceCommandTreeViewModel>();
        }

        //这个方法是模块加载后执行的系列动作，可以从容器中获取到注册的类然后注入到主项目的区域中
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("DeviceConfigRegion", typeof(DeviceConfigView));
            //这里将设备模块中的设备命令树视图注册到主项目的SidebarTreeRegion区域中
            regionManager.RegisterViewWithRegion("SidebarTreeRegion", typeof(DeviceCommandTreeView));
        }
    }
}