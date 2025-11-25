using BMES.Contracts.Events;
using BMES.Contracts.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace BMES.Modules.ProductionViewer.ViewModels
{
    public class ProductionViewerViewModel : BindableBase, INavigationAware, IDisposable
    {
        private readonly IOpcUaManager _opcUaManager;
        private readonly ITagConfigurationService _tagConfigService;
        private readonly IEventAggregator _eventAggregator;
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

        public ProductionViewerViewModel(IUnityContainer container)
        {
            //_opcUaManager = opcUaManager;
            _opcUaManager = container.Resolve<IOpcUaManager>();
            _tagConfigService = container.Resolve<ITagConfigurationService>();
            _eventAggregator = container.Resolve<IEventAggregator>();
            //_tagConfigService = tagConfigService;
            //_eventAggregator = eventAggregator;

            ConnectCommand = new DelegateCommand(async () => await ConnectAsync());
            ToggleFc1ForwardCommand = new DelegateCommand(async () => await ToggleFc1Forward());
            SetFc2ForwardCommand = new DelegateCommand(async () => await SetFc2Forward());
            SetFc2BackwardCommand = new DelegateCommand(async () => await SetFc2Backward());
            SetFc2StopCommand = new DelegateCommand(async () => await SetFc2Stop());

            _eventAggregator.GetEvent<TagValueChangedEvent>().Subscribe(OnTagValueChanged);
        }

        private void OnTagValueChanged(TagValue tagValue)
        {
            var tagName = _tagConfigService.GetNodeId(tagValue.NodeId);
            if (string.IsNullOrEmpty(tagName))
                return;

            switch (tagName)
            {
                case "TempOut":
                    OutdoorTemperature = Convert.ToDouble(tagValue.Value.Value) / 10;
                    break;
                case "MidTemp":
                    IndoorTemperature = Convert.ToDouble(tagValue.Value.Value) / 10;
                    break;
                case "Hudimity":
                    TvoHumidity = Convert.ToInt32(tagValue.Value.Value) / 10;
                    break;
                case "fc1Forward":
                    Fc1Forward = Convert.ToBoolean(tagValue.Value.Value);
                    break;
                case "fc2Forward":
                    Fc2Forward = Convert.ToBoolean(tagValue.Value.Value);
                    break;
                case "fc2Reverse":
                    Fc2Backward = Convert.ToBoolean(tagValue.Value.Value);
                    break;
                case "fc2Stop":
                    Fc2Stop = Convert.ToBoolean(tagValue.Value.Value);
                    break;
            }
        }

        private async Task ToggleFc1Forward()
        {
            try
            {
                await _opcUaManager.WriteAsync(_tagConfigService.GetNodeId("fc1Forward"), !Fc1Forward);
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
                    _opcUaManager.WriteAsync(_tagConfigService.GetNodeId("fc2Forward"), true),
                    _opcUaManager.WriteAsync(_tagConfigService.GetNodeId("fc2Reverse"), false),
                    _opcUaManager.WriteAsync(_tagConfigService.GetNodeId("fc2Stop"), false)
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
                    _opcUaManager.WriteAsync(_tagConfigService.GetNodeId("fc2Forward"), false),
                    _opcUaManager.WriteAsync(_tagConfigService.GetNodeId("fc2Reverse"), true),
                    _opcUaManager.WriteAsync(_tagConfigService.GetNodeId("fc2Stop"), false)
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
                    _opcUaManager.WriteAsync(_tagConfigService.GetNodeId("fc2Forward"), false),
                    _opcUaManager.WriteAsync(_tagConfigService.GetNodeId("fc2Reverse"), false),
                    _opcUaManager.WriteAsync(_tagConfigService.GetNodeId("fc2Stop"), true)
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
                    SubscribeToTags();
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
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            IsConnected = _opcUaManager.IsConnected;
            if (IsConnected)
            {
                SubscribeToTags();
            }
        }

        private void SubscribeToTags()
        {
            try
            {
                _opcUaManager.SubscribeToTag(_tagConfigService.GetNodeId("TempOut"));
                _opcUaManager.SubscribeToTag(_tagConfigService.GetNodeId("MidTemp"));
                _opcUaManager.SubscribeToTag(_tagConfigService.GetNodeId("Hudimity"));
                _opcUaManager.SubscribeToTag(_tagConfigService.GetNodeId("fc1Forward"));
                _opcUaManager.SubscribeToTag(_tagConfigService.GetNodeId("fc2Forward"));
                _opcUaManager.SubscribeToTag(_tagConfigService.GetNodeId("fc2Reverse"));
                _opcUaManager.SubscribeToTag(_tagConfigService.GetNodeId("fc2Stop"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to subscribe to OPC UA tags: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Dispose()
        {
            _eventAggregator.GetEvent<TagValueChangedEvent>().Unsubscribe(OnTagValueChanged);
        }
    }
}
