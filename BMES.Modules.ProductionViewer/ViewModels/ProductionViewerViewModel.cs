using BMES.Core.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BMES.Modules.ProductionViewer.ViewModels
{
    public class ProductionViewerViewModel : BindableBase, INavigationAware, IDisposable
    {
        public const string PLCTag = "ModbusPLC.PLC";

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

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set { SetProperty(ref _isConnected, value); }
        }

        public DelegateCommand ConnectCommand { get; private set; }

        public ProductionViewerViewModel(IOpcUaManager opcUaManager)
        {
            _opcUaManager = opcUaManager;
            ConnectCommand = new DelegateCommand(async () => await ConnectAsync());
        }

        private async Task ConnectAsync()
        {
            try
            {
                await _opcUaManager.ConnectAsync();
                IsConnected = _opcUaManager.IsConnected;
                if (IsConnected)
                {
                    await SubscribeToTags();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to OPC UA server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            // Dispose all subscriptions when navigating away
            Dispose();
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            IsConnected = _opcUaManager.IsConnected;
            if (IsConnected)
            {
                await SubscribeToTags();
            }
        }

        private async Task SubscribeToTags()
        {
            try
            {
                _subscriptions.Add((await _opcUaManager.SubscribeAsync<double>($"ns=2;s={PLCTag}.TempOut"))
                    .Subscribe(value => OutdoorTemperature = value / 10));

                _subscriptions.Add((await _opcUaManager.SubscribeAsync<double>($"ns=2;s={PLCTag}.MidTemp"))
                    .Subscribe(value => IndoorTemperature = value / 10));

                _subscriptions.Add((await _opcUaManager.SubscribeAsync<int>($"ns=2;s={PLCTag}.Hudimity"))
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
