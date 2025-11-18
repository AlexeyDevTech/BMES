using BMES.Core.Interfaces;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;

namespace BMES.Modules.ProductionViewer.ViewModels
{
    public class ProductionViewerViewModel : BindableBase, INavigationAware, IDisposable
    {
        private readonly IOpcUaManager _opcUaManager;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        private double _outdoorTemperature;
        public double OutdoorTemperature
        {
            get => _outdoorTemperature;
            set { SetProperty(ref _outdoorTemperature, value); }
        }

        private double _indoorTemperature;
        public double IndoorTemperature
        {
            get => _indoorTemperature;
            set { SetProperty(ref _indoorTemperature, value); }
        }

        private int _tvoHumidity;
        public int TvoHumidity
        {
            get => _tvoHumidity;
            set { SetProperty(ref _tvoHumidity, value); }
        }

        public ProductionViewerViewModel(IOpcUaManager opcUaManager)
        {
            _opcUaManager = opcUaManager;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            // Dispose all subscriptions when navigating away
            Dispose();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            try
            {
                _subscriptions.Add(_opcUaManager.Subscribe<double>("ns=2;s=OutdoorTemperature")
                    .Subscribe(value => OutdoorTemperature = value));

                _subscriptions.Add(_opcUaManager.Subscribe<double>("ns=2;s=IndoorTemperature")
                    .Subscribe(value => IndoorTemperature = value));

                _subscriptions.Add(_opcUaManager.Subscribe<int>("ns=2;s=TvoHumidity")
                    .Subscribe(value => TvoHumidity = value));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to subscribe to OPC UA tags: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Dispose()
        {
            foreach (var sub in _subscriptions)
            {
                sub.Dispose();
            }
            _subscriptions.Clear();
        }
    }
}
