using Prism.Ioc;
using Prism.Modularity;
// Removed using Prism.Regions;
using BMES.Contracts.Constants; // For RegionNames

namespace BMES.Modules.Recipes
{
    public class RecipesModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>(); // Use fully qualified name
            // For now, no specific view to navigate to.
            // regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(Views.RecipesView));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register types if any are specific to this module
        }
    }
}
