using Prism.Mvvm;
using BMES.Contracts.Interfaces;
using BMES.Core.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Events;
using System.Collections.Generic;

namespace BMES.Modules.ProductionViewer.ViewModels
{
    public class ProductionOrdersViewModel : BindableBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IWorkflowEngine _workflowEngine;

        private ObservableCollection<ProductionOrder> _productionOrders;
        public ObservableCollection<ProductionOrder> ProductionOrders
        {
            get => _productionOrders;
            set => SetProperty(ref _productionOrders, value);
        }

        private ProductionOrder _selectedProductionOrder;
        public ProductionOrder SelectedProductionOrder
        {
            get => _selectedProductionOrder;
            set
            {
                if (SetProperty(ref _selectedProductionOrder, value))
                {
                    StartWorkflowCommand.RaiseCanExecuteChanged();
                    PauseWorkflowCommand.RaiseCanExecuteChanged();
                    CancelWorkflowCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public DelegateCommand LoadProductionOrdersCommand { get; private set; }
        public DelegateCommand<ProductionOrder> StartWorkflowCommand { get; private set; }
        public DelegateCommand<ProductionOrder> PauseWorkflowCommand { get; private set; }
        public DelegateCommand<ProductionOrder> CancelWorkflowCommand { get; private set; }

        public ProductionOrdersViewModel(IOrderRepository orderRepository, IWorkflowEngine workflowEngine)
        {
            _orderRepository = orderRepository;
            _workflowEngine = workflowEngine;

            ProductionOrders = new ObservableCollection<ProductionOrder>();
            LoadProductionOrdersCommand = new DelegateCommand(async () => await ExecuteLoadProductionOrdersCommand());
            StartWorkflowCommand = new DelegateCommand<ProductionOrder>(async (order) => await ExecuteStartWorkflowCommand(order), (order) => CanExecuteWorkflowCommand(order));
            PauseWorkflowCommand = new DelegateCommand<ProductionOrder>((order) => ExecutePauseWorkflowCommand(order), (order) => CanExecuteWorkflowCommand(order));
            CancelWorkflowCommand = new DelegateCommand<ProductionOrder>((order) => ExecuteCancelWorkflowCommand(order), (order) => CanExecuteWorkflowCommand(order));

            LoadProductionOrdersCommand.Execute();
            if (ProductionOrders.Count == 0)
            {
                _orderRepository.AddOrderAsync(new ProductionOrder { OrderNumber = "PO-001", ProductName = "Product A", Quantity = 100, Status = OrderStatus.New });
                _orderRepository.AddOrderAsync(new ProductionOrder { OrderNumber = "PO-002", ProductName = "Product B", Quantity = 150, Status = OrderStatus.New });
                LoadProductionOrdersCommand.Execute();
            }
        }

        private async Task ExecuteLoadProductionOrdersCommand()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            ProductionOrders.Clear();
            foreach (var order in orders)
            {
                ProductionOrders.Add(order);
            }
        }

        private bool CanExecuteWorkflowCommand(ProductionOrder order)
        {
            return order != null;
        }

        private async Task ExecuteStartWorkflowCommand(ProductionOrder order)
        {
            if (order != null)
            {
                var dummyWorkflow = new ProcessWorkflow
                {
                    Name = $"Workflow for {order.OrderNumber}",
                    Description = "Dummy workflow",
                    Steps = new List<WorkflowStep>
                    {
                        new ControlStep { Order = 1, Name = "Start Mixing", TagName = "Mixer.Start", Value = true },
                        new MonitorStep { Order = 2, Name = "Wait for Temperature", TagName = "Tank.Temperature", Condition = "Value > 50" },
                        new OperatorInstructionStep { Order = 3, Name = "Verify Batch", InstructionText = "Check quality and confirm." },
                        new DataCollectionStep { Order = 4, Name = "Log Data", TagName = "Batch.Data", DataPointName = "BatchData" }
                    }
                };
                await _workflowEngine.StartWorkflow(dummyWorkflow, order);
                await ExecuteLoadProductionOrdersCommand();
            }
        }

        private void ExecutePauseWorkflowCommand(ProductionOrder order)
        {
            if (order != null)
            {
                _workflowEngine.PauseWorkflow(order);
            }
        }

        private void ExecuteCancelWorkflowCommand(ProductionOrder order)
        {
            if (order != null)
            {
                _workflowEngine.CancelWorkflow(order);
                LoadProductionOrdersCommand.Execute();
            }
        }
    }
}
