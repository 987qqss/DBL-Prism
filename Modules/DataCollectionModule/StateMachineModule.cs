using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;
using StateMachineModule.ViewModels;
using StateMachineModule.Views;

namespace StateMachineModule
{
    public class StateMachineModule : IModule//数据采集模块
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<CommandRunView, CommandRunViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("CommandRunRegion", typeof(CommandRunView));
        }
    }
}