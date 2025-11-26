using System;
using System.Linq;
using System.Windows;
using BMES.Contracts.Interfaces;
using BMES.Core;
using BMES.Infrastructure;
using BMES.Infrastructure.Persistence;
using BMES.Infrastructure.Repositories;
using BMES.Infrastructure.Services;
using BMES.Modules.Alarms;
using BMES.Modules.Genealogy;
using BMES.Modules.Header;
using BMES.Modules.LeftMenu;
using BMES.Modules.Mimic;
using BMES.Modules.ProductionViewer;
using BMES.Modules.Recipes;
using BMES.Modules.WorkflowEditor;
using BMES.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Opc.Ua.Client;
using Prism.Ioc;
using Prism.Modularity;
using Serilog;
//using Opc.Ua.Client.Session;

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

            // Register ILogger<T>
            containerRegistry.Register(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(BMES.Core.LoggerAdapter<>));

            // OPC UA Services
            containerRegistry.RegisterSingleton<IOpcUaClient, OpcUaClient>();
            containerRegistry.RegisterSingleton<IOpcUaManager, OpcUaManager>();
            containerRegistry.RegisterSingleton<IOpcUaService, OpcUaManager>();

            // EF Core DbContext
            var optionsBuilder = new DbContextOptionsBuilder<BmesDbContext>();
            optionsBuilder.UseInMemoryDatabase("BmesDb");
            containerRegistry.RegisterInstance<DbContextOptions<BmesDbContext>>(optionsBuilder.Options);
            containerRegistry.Register<BmesDbContext>();

            // Repositories
            containerRegistry.Register<IOrderRepository, OrderRepository>();
            containerRegistry.Register<IAlarmRepository, AlarmRepository>();
            containerRegistry.Register<IRecipeRepository, RecipeRepository>();
            containerRegistry.Register<IMaterialLotRepository, MaterialLotRepository>();
            containerRegistry.Register<IProcessWorkflowRepository, ProcessWorkflowRepository>();
            containerRegistry.Register<IEquipmentStateLogRepository, EquipmentStateLogRepository>();

            // Services
            containerRegistry.RegisterSingleton<ITagConfigurationService, TagConfigurationService>();
            containerRegistry.RegisterSingleton<ICurrentUserService, CurrentUserService>();
            containerRegistry.RegisterSingleton<IAuditService, AuditService>();
            containerRegistry.RegisterSingleton<IWorkflowEngine, WorkflowEngine>();
            containerRegistry.RegisterSingleton<IOeeCalculatorService, OeeCalculatorService>();
            containerRegistry.RegisterSingleton<IProductionReportService, ProductionReportService>();
        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
            moduleCatalog.AddModule<HeaderModule>();
            moduleCatalog.AddModule<LeftMenuModule>();
            moduleCatalog.AddModule<ProductionViewerModule>();
            moduleCatalog.AddModule<MimicModule>();
            moduleCatalog.AddModule<AlarmsModule>();
            moduleCatalog.AddModule<RecipesModule>();
            moduleCatalog.AddModule<GenealogyModule>();
            moduleCatalog.AddModule<WorkflowEditorModule>();
        }
    }
}
