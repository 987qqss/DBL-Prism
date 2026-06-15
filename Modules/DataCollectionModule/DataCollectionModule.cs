using Prism.Ioc;
using Prism.Modularity;
using DataCollectionModule.Views;
using DataCollectionModule.ViewModels;

namespace DataCollectionModule
{
    public class DataCollectionModule : IModule//数据采集模块
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DataCollectionView, DataCollectionViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}