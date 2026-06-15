using Prism.Ioc;
using Prism.Modularity;
using SettingsModule.Views;
using SettingsModule.ViewModels;

namespace SettingsModule
{
    public class SettingsModule : IModule//系统设置模块
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<SettingsView, SettingsViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}