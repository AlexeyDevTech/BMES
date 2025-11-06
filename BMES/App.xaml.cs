using System.Windows;
using BMES.Core;
using BMES.Core.Interfaces;
using BMES.Modules.Header;
using BMES.Modules.LeftMenu;
using BMES.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace BMES
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterSingleton<IOpcUaClient, OpcUaClient>();
        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
            moduleCatalog.AddModule<HeaderModule>();
            moduleCatalog.AddModule<LeftMenuModule>();
        }
    }
}
