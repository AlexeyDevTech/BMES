using BMES.Modules.Header.Views;
using BMES.Regions;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace BMES.Modules.Header
{
    public class HeaderModule : IModule
    {
        private IRegionManager _reg;

        public HeaderModule(IRegionManager reg) 
        { 
            _reg = reg;
        }
        public void OnInitialized(IContainerProvider containerProvider)
        {
            _reg.RegisterViewWithRegion(RegionNames.TopMenu, containerProvider.Resolve<HeaderView>);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}