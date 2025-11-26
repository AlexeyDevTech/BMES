using BMES.Modules.Alarms.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace BMES.Modules.Alarms
{
    public class AlarmsModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
 
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<AlarmsView>();
        }
    }
}