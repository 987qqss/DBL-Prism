using Prism.Ioc;
using Prism.Modularity;
using AlarmModule.Views;
using AlarmModule.ViewModels;

namespace AlarmModule
{
    public class AlarmModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<AlarmView, AlarmViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}