using Prism.Ioc;
using Prism.Modularity;
using ReportModule.Views;
using ReportModule.ViewModels;

namespace ReportModule
{
    public class ReportModule : IModule//报表统计模块
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ReportView, ReportViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}