using BMES.Regions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Windows.Input;

namespace BMES.Modules.LeftMenu.ViewModels
{
    public class LeftMenuViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        public DelegateCommand<string> NavigateCommand { get; private set; }

        public LeftMenuViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            NavigateCommand = new DelegateCommand<string>(Navigate);
        }

        private void Navigate(string navigationPath)
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, navigationPath);
        }
    }
}
