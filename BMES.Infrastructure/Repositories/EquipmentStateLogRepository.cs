using BMES.Contracts.Interfaces;
using BMES.Core.Models;
using BMES.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BMES.Infrastructure.Repositories
{
    public class EquipmentStateLogRepository : IEquipmentStateLogRepository
    {
        private readonly BmesDbContext _context;

        public EquipmentStateLogRepository(BmesDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EquipmentStateLog>> GetAllEquipmentStateLogsAsync()
        {
            return await _context.EquipmentStateLogs.ToListAsync();
        }

        public async Task<IEnumerable<EquipmentStateLog>> GetEquipmentStateLogsByEquipmentIdAsync(string equipmentId, DateTime startDate, DateTime endDate)
        {
            return await _context.EquipmentStateLogs
                                 .Where(log => log.EquipmentId == equipmentId &&
                                               log.Timestamp >= startDate &&
                                               log.Timestamp <= endDate)
                                 .OrderBy(log => log.Timestamp)
                                 .ToListAsync();
        }

        public async Task AddEquipmentStateLogAsync(EquipmentStateLog log)
        {
            await _context.EquipmentStateLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
