using Prism.Ioc;
using Prism.Modularity;
using OperationModule.Views;
using OperationModule.ViewModels;

namespace OperationModule
{
    public class OperationModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<OperateView, OperateViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}