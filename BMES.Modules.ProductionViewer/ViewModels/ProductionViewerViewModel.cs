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

        private bool _fc1Forward;
        private bool _fc2Forward;
        private bool _fc2Backward;
        private bool _fc2Stop;

        public bool Fc1Forward
        {
            get => _fc1Forward;
            set { SetProperty(ref _fc1Forward, value); }
        }
        public bool Fc2Forward
        {
            get => _fc2Forward;
            set { SetProperty(ref _fc2Forward, value); }
        }
        public bool Fc2Backward
        {
            get => _fc2Backward;
            set { SetProperty(ref _fc2Backward, value); }
        }
        public bool Fc2Stop
        {
            get => _fc2Stop;
            set { SetProperty(ref _fc2Stop, value); }
        }

        public DelegateCommand ConnectCommand { get; private set; }
        public DelegateCommand ToggleFc1ForwardCommand { get; private set; }
        public DelegateCommand SetFc2ForwardCommand { get; private set; }
        public DelegateCommand SetFc2BackwardCommand { get; private set; }
        public DelegateCommand SetFc2StopCommand { get; private set; }

        public ProductionViewerViewModel(IOpcUaManager opcUaManager)
        {
            _opcUaManager = opcUaManager;
            ConnectCommand = new DelegateCommand(async () => await ConnectAsync());
            ToggleFc1ForwardCommand = new DelegateCommand(async () => await ToggleFc1Forward());
            SetFc2ForwardCommand = new DelegateCommand(async () => await SetFc2Forward());
            SetFc2BackwardCommand = new DelegateCommand(async () => await SetFc2Backward());
            SetFc2StopCommand = new DelegateCommand(async () => await SetFc2Stop());

        }

        private async Task ToggleFc1Forward()
        {
            try
            {
                await _opcUaManager.WriteAsync($"ns=2;s={PLCTag}.fc1Forward", !Fc1Forward);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to write to fc1Forward tag: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SetFc2Forward()
        {
            try
            {
                var tasks = new List<Task>
                {
                    _opcUaManager.WriteAsync($"ns=2;s={PLCTag}.fc2Forward", true),
                    _opcUaManager.WriteAsync($"ns=2;s={PLCTag}.fc2Reverse", false),
                    _opcUaManager.WriteAsync($"ns=2;s={PLCTag}.fc2Stop", false)
                };
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set fc2 state: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SetFc2Backward()
        {
            try
            {
                var tasks = new List<Task>
                {
                    _opcUaManager.WriteAsync($"ns=2;s={PLCTag}.fc2Forward", false),
                    _opcUaManager.WriteAsync($"ns=2;s={PLCTag}.fc2Reverse", true),
                    _opcUaManager.WriteAsync($"ns=2;s={PLCTag}.fc2Stop", false)
                };
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set fc2 state: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SetFc2Stop()
        {
            try
            {
                var tasks = new List<Task>
                {
                    _opcUaManager.WriteAsync($"ns=2;s={PLCTag}.fc2Forward", false),
                    _opcUaManager.WriteAsync($"ns=2;s={PLCTag}.fc2Reverse", false),
                    _opcUaManager.WriteAsync($"ns=2;s={PLCTag}.fc2Stop", true)
                };
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set fc2 state: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                    .Subscribe(value => TvoHumidity = value / 10));

                _subscriptions.Add((await _opcUaManager.SubscribeAsync<bool>($"ns=2;s={PLCTag}.fc1Forward"))
                    .Subscribe(value => Fc1Forward = value));

                _subscriptions.Add((await _opcUaManager.SubscribeAsync<bool>($"ns=2;s={PLCTag}.fc2Forward"))
                    .Subscribe(value => Fc2Forward = value));

                _subscriptions.Add((await _opcUaManager.SubscribeAsync<bool>($"ns=2;s={PLCTag}.fc2Reverse"))
                    .Subscribe(value => Fc2Backward = value));

                _subscriptions.Add((await _opcUaManager.SubscribeAsync<bool>($"ns=2;s={PLCTag}.fc2Stop"))
                    .Subscribe(value => Fc2Stop = value));
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
