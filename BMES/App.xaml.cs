using BMES.Core;
using BMES.Core.Interfaces;
using BMES.Modules.Header;
using BMES.Modules.LeftMenu;
using BMES.Modules.ProductionViewer;
using BMES.Views;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug; // Added this line

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
            // Configure logging
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddDebug()
                    .SetMinimumLevel(LogLevel.Trace);
            });

            containerRegistry.RegisterInstance<ILoggerFactory>(loggerFactory);
            containerRegistry.Register(typeof(ILogger<>), typeof(Logger<>));

            containerRegistry.RegisterSingleton<IOpcUaService, OpcUaService>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            var logger = Container.Resolve<ILogger<App>>();

            try
            {
                var opcUaService = Container.Resolve<IOpcUaService>();
                _ = opcUaService.StartAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to start OPC UA service.");
                MessageBox.Show($"Failed to start OPC UA service: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
            moduleCatalog.AddModule<HeaderModule>();
            moduleCatalog.AddModule<LeftMenuModule>();
            moduleCatalog.AddModule<ProductionViewerModule>();
        }
    }
}
