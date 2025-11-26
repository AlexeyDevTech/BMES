using BMES.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BMES.Contracts.Interfaces
{
    public interface IEquipmentStateLogRepository
    {
        Task<IEnumerable<EquipmentStateLog>> GetAllEquipmentStateLogsAsync();
        Task<IEnumerable<EquipmentStateLog>> GetEquipmentStateLogsByEquipmentIdAsync(string equipmentId, DateTime startDate, DateTime endDate);
        Task AddEquipmentStateLogAsync(EquipmentStateLog log);
    }
}
