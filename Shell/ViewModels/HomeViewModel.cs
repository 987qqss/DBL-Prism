using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace Shell.ViewModels
{
    public class HomeViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;

        public HomeViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (_regionManager.Regions.ContainsRegionWithName("CommandRunRegion"))
            {
                var cmdRegion = _regionManager.Regions["CommandRunRegion"];
                if (!cmdRegion.Views.Any())
                {
                    _regionManager.RegisterViewWithRegion("CommandRunRegion", typeof(Shell.Views.CommandRunView));
                }
            }

            if (_regionManager.Regions.ContainsRegionWithName("LogRegion"))
            {
                var logRegion = _regionManager.Regions["LogRegion"];
                if (!logRegion.Views.Any())
                {
                    _regionManager.RegisterViewWithRegion("LogRegion", typeof(LogModule.Views.LogView));
                }
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}