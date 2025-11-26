using BMES.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BMES.Contracts.Interfaces
{
    public interface IAlarmRepository
    {
        Task<IEnumerable<AlarmEvent>> GetAllAlarmsAsync();
        Task UpdateAlarmAsync(AlarmEvent alarm);
        Task AddAlarmAsync(AlarmEvent alarm);
    }
}
