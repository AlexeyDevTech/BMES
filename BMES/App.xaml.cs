using System.Windows;
using BMES.Core;
using BMES.Core.Interfaces;
using BMES.Views;
using Prism.Ioc;

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
            containerRegistry.RegisterSingleton<IOpcUaClient, OpcUaClient>();
        }
    }
}
