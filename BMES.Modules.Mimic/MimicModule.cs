using BMES.Modules.Mimic.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace BMES.Modules.Mimic
{
    public class MimicModule: IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
 
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MimicView>();
        }
    }
}