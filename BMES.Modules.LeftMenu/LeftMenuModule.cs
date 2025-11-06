using BMES.Modules.LeftMenu.Views;
using BMES.Regions;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace BMES.Modules.LeftMenu
{
    public class LeftMenuModule : IModule
    {
        private IRegionManager _reg;

        public LeftMenuModule(IRegionManager reg)
        {
            _reg = reg;
        }
        public void OnInitialized(IContainerProvider containerProvider)
        {
            _reg.RegisterViewWithRegion(RegionNames.LeftMenu, containerProvider.Resolve<LeftMenuView>);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}