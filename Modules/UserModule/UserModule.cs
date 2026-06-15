using Prism.Ioc;
using Prism.Modularity;
using UserModule.Views;
using UserModule.ViewModels;

namespace UserModule
{
    public class UserModule : IModule//用户管理模块
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<UserView, UserViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}