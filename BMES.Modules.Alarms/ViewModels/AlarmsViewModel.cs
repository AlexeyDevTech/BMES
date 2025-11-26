using BMES.Contracts.Events;
using BMES.Core.Models;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows;
using Prism.Commands;
using BMES.Contracts.Interfaces;
using BMES.Contracts.Commands;
using System.Linq;
using System.Threading.Tasks;


namespace BMES.Modules.Alarms.ViewModels
{
    public class AlarmsViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IAlarmRepository _alarmRepository;
        private readonly IOpcUaService _opcUaService;
        public ObservableCollection<AlarmEvent> Alarms { get; } = new ObservableCollection<AlarmEvent>();

        private AlarmEvent _selectedAlarm;
        public AlarmEvent SelectedAlarm
        {
            get => _selectedAlarm;
            set => SetProperty(ref _selectedAlarm, value);
        }

        public DelegateCommand AcknowledgeAlarmCommand { get; private set; }

        public AlarmsViewModel(IEventAggregator eventAggregator, IAlarmRepository alarmRepository, IOpcUaService opcUaService)
        {
            _eventAggregator = eventAggregator;
            _alarmRepository = alarmRepository;
            _opcUaService = opcUaService;

            _eventAggregator.GetEvent<AlarmEventTriggered>().Subscribe(OnAlarmEventTriggered, ThreadOption.UIThread);

            AcknowledgeAlarmCommand = new DelegateCommand(async () => await OnAcknowledgeAlarm(), CanAcknowledgeAlarm);
            ApplicationCommands.AcknowledgeAlarmCommand.RegisterCommand(AcknowledgeAlarmCommand);
            LoadAlarmsAsync();
        }

        private async void LoadAlarmsAsync()
        {
            var alarms = await _alarmRepository.GetAllAlarmsAsync();
            foreach (var alarm in alarms.OrderByDescending(a => a.Timestamp))
            {
                Alarms.Add(alarm);
            }
        }

        private void OnAlarmEventTriggered(AlarmEvent alarm)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Alarms.Insert(0, alarm);
            });
        }

        private bool CanAcknowledgeAlarm()
        {
            return SelectedAlarm != null && !SelectedAlarm.IsAcknowledged;
        }

        private async Task OnAcknowledgeAlarm()
        {
            if (SelectedAlarm != null)
            {
                SelectedAlarm.IsAcknowledged = true;
                await _alarmRepository.UpdateAlarmAsync(SelectedAlarm);

                if (!string.IsNullOrEmpty(SelectedAlarm.NodeId))
                {
                    await _opcUaService.WriteTagAsync(SelectedAlarm.NodeId, false);
                }

                AcknowledgeAlarmCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
