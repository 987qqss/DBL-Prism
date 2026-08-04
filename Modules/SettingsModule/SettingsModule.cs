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
            // transient：每次打开设置窗口解析新实例
            containerRegistry.Register<SettingsViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}