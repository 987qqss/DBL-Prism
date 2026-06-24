using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;
using LogModule.Views;
using LogModule.ViewModels;
using LogModule.Services;
using Core.Interfaces;
using NLog;
using System.Collections.Specialized;

namespace LogModule
{
    public class LogModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //注册全局单例日志服务接口
            containerRegistry.RegisterSingleton<ILogService, LogService>();
            //注册日志窗口到容器（支持导航和区域注入）
            containerRegistry.RegisterForNavigation<LogView, LogViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();

            // 监听区域集合变化：当 LogRegion 被创建（HomeView 加载后）时，自动导航 LogView 进去
            // 相比 RegisterViewWithRegion，此方式保证区域一定存在时才注入
            regionManager.Regions.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
                {
                    foreach (IRegion region in e.NewItems)
                    {
                        if (region.Name == "LogRegion")
                        {
                            regionManager.RequestNavigate("LogRegion", "LogView");
                        }
                    }
                }
            };
        }
    }
}
