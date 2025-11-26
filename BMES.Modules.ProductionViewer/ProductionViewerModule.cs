using BMES.Modules.ProductionViewer.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Navigation.Regions;

namespace BMES.Modules.ProductionViewer
{
    public class ProductionViewerModule : IModule
    {
        private IRegionManager _reg;

        public ProductionViewerModule(IRegionManager reg)
        {
            _reg = reg;
        }
        public void OnInitialized(IContainerProvider containerProvider)
        {
            _reg.RegisterViewWithRegion("ContentRegion", typeof(ProductionOrdersView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ProductionOrdersView>("ProductionOrders");
        }
    }
}