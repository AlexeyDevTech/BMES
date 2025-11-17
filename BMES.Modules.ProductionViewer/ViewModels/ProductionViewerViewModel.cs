using BMES.Core.Interfaces;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.ComponentModel;

namespace BMES.Modules.ProductionViewer.ViewModels
{
    public class ProductionViewerViewModel : BindableBase, INavigationAware, IDisposable
    {
        private readonly IOpcUaService _opcUaService;

        private float _outdoorTemperature;
        private float _indoorTemperature;
        private float _tvoHumidity;

        public float OutdoorTemperature
        {
            get => _outdoorTemperature;
            set { SetProperty(ref _outdoorTemperature, value); }
        }

        public float IndoorTemperature
        {
            get => _indoorTemperature;
            set { SetProperty(ref _indoorTemperature, value); }
        }

        public float TvoHumidity
        {
            get => _tvoHumidity;
            set { SetProperty(ref _tvoHumidity, value); }
        }

        public ProductionViewerViewModel(IOpcUaService opcUaService)
        {
            _opcUaService = opcUaService;
            _opcUaService.PropertyChanged += OnOpcUaServicePropertyChanged;

            // Initialize with current values
            IndoorTemperature = _opcUaService.MidTemp;
            OutdoorTemperature = _opcUaService.TempOut;
            TvoHumidity = _opcUaService.Hudimity;
        }

        private void OnOpcUaServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IOpcUaService.MidTemp):
                    IndoorTemperature = _opcUaService.MidTemp;
                    break;
                case nameof(IOpcUaService.TempOut):
                    OutdoorTemperature = _opcUaService.TempOut;
                    break;
                case nameof(IOpcUaService.Hudimity):
                    TvoHumidity = _opcUaService.Hudimity;
                    break;
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public void Dispose()
        {
            _opcUaService.PropertyChanged -= OnOpcUaServicePropertyChanged;
        }
    }
}
