using Prism.Ioc;
using Prism.Modularity;
// Removed using Prism.Regions;
using BMES.Contracts.Constants; // For RegionNames

namespace BMES.Modules.Genealogy
{
    public class GenealogyModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>(); // Use fully qualified name
            // No specific view to navigate to yet.
            // regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.GenealogyView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register types if any are specific to this module
        }
    }
}
