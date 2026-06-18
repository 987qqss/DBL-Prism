using AlarmModule.Views;
using AlarmModule.ViewModels;

namespace AlarmModule
{
    public class AlarmModule : IModule//报警模块
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //加载报警视图和ViewModel到容器中
            containerRegistry.RegisterForNavigation<AlarmView, AlarmViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}