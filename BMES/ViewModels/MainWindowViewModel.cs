using Prism.Mvvm;
using Prism.Commands;
using BMES.Contracts.Interfaces;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using BMES.Core.Models;
using System.Linq;
using System;

namespace BMES.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private readonly ICurrentUserService _currentUser;
        private readonly IAuditService _auditService;
        private readonly IOpcUaService _opcUaService;
        private readonly ITagConfigurationService _tagConfigurationService;
        private readonly IMaterialLotRepository _materialLotRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProductionReportService _productionReportService;

        public DelegateCommand StartMotorCommand { get; private set; }

        private ObservableCollection<MaterialLot> _materialLots;
        public ObservableCollection<MaterialLot> MaterialLots
        {
            get => _materialLots;
            set => SetProperty(ref _materialLots, value);
        }

        private MaterialLot _selectedMaterialLot;
        public MaterialLot SelectedMaterialLot
        {
            get => _selectedMaterialLot;
            set
            {
                if (SetProperty(ref _selectedMaterialLot, value))
                {
                    AssignMaterialLotCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private ProductionOrder _selectedProductionOrder;
        public ProductionOrder SelectedProductionOrder
        {
            get => _selectedProductionOrder;
            set
            {
                if (SetProperty(ref _selectedProductionOrder, value))
                {
                    AssignMaterialLotCommand.RaiseCanExecuteChanged();
                    RecordQualityCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private double _qualityInput;
        public double QualityInput
        {
            get => _qualityInput;
            set
            {
                if (SetProperty(ref _qualityInput, value))
                {
                    RecordQualityCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private int _reportShiftNumber = 1;
        public int ReportShiftNumber
        {
            get => _reportShiftNumber;
            set
            {
                if (SetProperty(ref _reportShiftNumber, value))
                {
                    GenerateReportCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private DateTime _reportDate = DateTime.Today;
        public DateTime ReportDate
        {
            get => _reportDate;
            set
            {
                if (SetProperty(ref _reportDate, value))
                {
                    GenerateReportCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public DelegateCommand LoadMaterialLotsCommand { get; private set; }
        public DelegateCommand AssignMaterialLotCommand { get; private set; }
        public DelegateCommand RecordQualityCommand { get; private set; }
        public DelegateCommand GenerateReportCommand { get; private set; }

        public MainWindowViewModel(ICurrentUserService currentUserService, 
            IAuditService auditService, 
            IOpcUaService opcUaService, 
            ITagConfigurationService tagConfigurationService, 
            IMaterialLotRepository materialLotRepository, 
            IOrderRepository orderRepository, 
            IProductionReportService productionReportService)
        {
            _currentUser = currentUserService;
            _auditService = auditService;
            _opcUaService = opcUaService;
            _tagConfigurationService = tagConfigurationService;
            _materialLotRepository = materialLotRepository;
            _orderRepository = orderRepository;
            _productionReportService = productionReportService;

            StartMotorCommand = new DelegateCommand(async () => await OnStartMotor(), CanStartMotor);
            LoadMaterialLotsCommand = new DelegateCommand(async () => await ExecuteLoadMaterialLotsCommand());
            AssignMaterialLotCommand = new DelegateCommand(async () => await ExecuteAssignMaterialLotCommand(), CanAssignMaterialLot);
            RecordQualityCommand = new DelegateCommand(async () => await ExecuteRecordQualityCommand(), CanRecordQuality);
            GenerateReportCommand = new DelegateCommand(async () => await ExecuteGenerateReportCommand(), CanGenerateReport);

            LoadMaterialLotsCommand.Execute();
            SelectedProductionOrder = new ProductionOrder { Id = 1, OrderNumber = "PO-001", ProductName = "Product A", Quantity = 100, Status = OrderStatus.InProgress, ProductionOrderCompletionTime = DateTime.Today };
        }

        private bool CanStartMotor()
        {
            return _currentUser.HasPermission("CanControlMotors");
        }

        private async Task OnStartMotor()
        {
            _auditService.Log(_currentUser.UserName, "Нажата кнопка 'Старт мотора'");
            
            string motorTagNodeId = _tagConfigurationService.GetNodeId("Motor1.Start");
            if (!string.IsNullOrEmpty(motorTagNodeId))
            {
                await _opcUaService.WriteTagAsync(motorTagNodeId, true);
            }
            else
            {
                _auditService.Log("System", "Ошибка: Тег 'Motor1.Start' не найден в конфигурации.");
            }
        }

        private async Task ExecuteLoadMaterialLotsCommand()
        {
            var lots = await _materialLotRepository.GetAllMaterialLotsAsync();
            MaterialLots = new ObservableCollection<MaterialLot>(lots);
        }

        private bool CanAssignMaterialLot()
        {
            return SelectedMaterialLot != null && SelectedProductionOrder != null;
        }

        private async Task ExecuteAssignMaterialLotCommand()
        {
            if (CanAssignMaterialLot())
            {
                _auditService.Log(_currentUser.UserName, $"Присвоение партии {SelectedMaterialLot.LotNumber} к заказу {SelectedProductionOrder.OrderNumber}");
                SelectedProductionOrder.MaterialLotId = SelectedMaterialLot.Id;
                await _orderRepository.UpdateOrderAsync(SelectedProductionOrder);
            }
        }

        private bool CanRecordQuality()
        {
            return SelectedProductionOrder != null && QualityInput >= 0;
        }

        private async Task ExecuteRecordQualityCommand()
        {
            if (CanRecordQuality())
            {
                _auditService.Log(_currentUser.UserName, $"Записано качество {QualityInput} для заказа {SelectedProductionOrder.OrderNumber}");
                await Task.CompletedTask;
            }
        }

        private bool CanGenerateReport()
        {
            return ReportShiftNumber >= 1 && ReportShiftNumber <= 3 && ReportDate != default;
        }

        private async Task ExecuteGenerateReportCommand()
        {
            if (CanGenerateReport())
            {
                _auditService.Log(_currentUser.UserName, $"Запрошен отчет по смене {ReportShiftNumber} за {ReportDate:d}");
                string reportPath = await _productionReportService.GenerateShiftReport(ReportShiftNumber, ReportDate);
                if (!string.IsNullOrEmpty(reportPath))
                {
                    _auditService.Log("System", $"Отчет успешно сгенерирован: {reportPath}");
                }
                else
                {
                    _auditService.Log("System", "Ошибка при генерации отчета.");
                }
            }
        }
    }
}
