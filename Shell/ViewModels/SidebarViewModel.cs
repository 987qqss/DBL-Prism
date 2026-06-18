using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using Shell.Models;
using System.Collections.ObjectModel;

namespace Shell.ViewModels
{
    //导航栏视图ViewModel
    public class SidebarViewModel : BindableBase
    {

        private readonly IRegionManager _regionManager;

        public ObservableCollection<MenuItemModel> NavItems { get; }

        public SidebarViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            NavItems = BuildNavItems();
        }

        //导航菜单命令集合
        private ObservableCollection<MenuItemModel> BuildNavItems() => new()
        {
            new MenuItemModel { Header = "设备信息", IconKind = "Monitor", Command = new DelegateCommand(() => NavigateTo("DeviceManagementView")) },
            new MenuItemModel { Header = "报警信息", IconKind = "Alert", Command = new DelegateCommand(() => NavigateTo("AlarmView")) },
            new MenuItemModel { Header = "参数下发", IconKind = "Download", Command = new DelegateCommand(() => NavigateTo("OperateView")) },
            new MenuItemModel { Header = "设备状态", IconKind = "History", Command = new DelegateCommand(() => NavigateTo("DeviceStateView")) },
            new MenuItemModel { Header = "状态机演示", IconKind = "StateMachine", Command = new DelegateCommand(() => NavigateTo("StateMachineView")) },
        };

        //根据传入的字符串导航到对应界面
        private void NavigateTo(string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName)) return;
            _regionManager.RequestNavigate("ContentRegion", viewName);
        }
    }
}