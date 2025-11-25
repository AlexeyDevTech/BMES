using System.Windows;
using BMES.Contracts.Interfaces;
using BMES.Infrastructure;
using BMES.Infrastructure.Persistence;
using BMES.Infrastructure.Repositories;
using BMES.Infrastructure.Services;
using BMES.Modules.Header;
using BMES.Modules.LeftMenu;
using BMES.Modules.ProductionViewer;
using BMES.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prism.Ioc;
using Prism.Modularity;
using Serilog;

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
            // Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/bmes-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            containerRegistry.RegisterInstance<ILoggerFactory>(new Serilog.Extensions.Logging.SerilogLoggerFactory());

            // OPC UA Services
            containerRegistry.RegisterSingleton<IOpcUaClient, OpcUaClient>();
            containerRegistry.RegisterSingleton<IOpcUaManager, OpcUaManager>();

            // EF Core DbContext
            var optionsBuilder = new DbContextOptionsBuilder<BmesDbContext>();
            optionsBuilder.UseInMemoryDatabase("BmesDb");
            containerRegistry.RegisterInstance<DbContextOptions<BmesDbContext>>(optionsBuilder.Options);
            containerRegistry.Register<BmesDbContext>();

            // Repositories
            containerRegistry.Register<IOrderRepository, OrderRepository>();

            // Services
            containerRegistry.RegisterSingleton<ITagConfigurationService, TagConfigurationService>();
        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
            moduleCatalog.AddModule<HeaderModule>();
            moduleCatalog.AddModule<LeftMenuModule>();
            moduleCatalog.AddModule<ProductionViewerModule>();
            moduleCatalog.AddModule<BMES.Modules.Mimic.MimicModule>();
            moduleCatalog.AddModule<BMES.Modules.Alarms.AlarmsModule>();
        }
    }
}
