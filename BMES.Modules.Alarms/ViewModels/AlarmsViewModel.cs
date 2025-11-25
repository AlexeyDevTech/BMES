using BMES.Contracts.Events;
using BMES.Core.Models;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows;

namespace BMES.Modules.Alarms.ViewModels
{
    public class AlarmsViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        public ObservableCollection<AlarmEvent> Alarms { get; } = new ObservableCollection<AlarmEvent>();

        public AlarmsViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<AlarmEventTriggered>().Subscribe(OnAlarmEventTriggered, ThreadOption.UIThread);
        }

        private void OnAlarmEventTriggered(AlarmEvent alarm)
        {
            Alarms.Add(alarm);
        }
    }
}
