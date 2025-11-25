using BMES.Core.Models;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace BMES.Modules.ProductionViewer.ViewModels
{
    public class ProductionOrderViewModel : BindableBase
    {
        private int _productionOrderId;
        public int ProductionOrderID
        {
            get => _productionOrderId;
            set => SetProperty(ref _productionOrderId, value);
        }

        private int _recipeId;
        public int RecipeID
        {
            get => _recipeId;
            set => SetProperty(ref _recipeId, value);
        }

        private DateTime? _startTime;
        public DateTime? StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        private DateTime? _endTime;
        public DateTime? EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        private ProductionOrderStatus _status;
        public ProductionOrderStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public ObservableCollection<ProductionOrderEvent> Events { get; } = new ObservableCollection<ProductionOrderEvent>();
    }

    public enum ProductionOrderStatus
    {
        New,
        InProgress,
        Completed,
        Cancelled,
        OnHold,
        Failed
    }
}
