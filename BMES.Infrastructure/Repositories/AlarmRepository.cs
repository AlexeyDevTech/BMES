using BMES.Contracts.Interfaces;
using BMES.Core.Models;
using BMES.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BMES.Infrastructure.Repositories
{
    public class AlarmRepository : IAlarmRepository
    {
        private readonly BmesDbContext _context;

        public AlarmRepository(BmesDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AlarmEvent>> GetAllAlarmsAsync()
        {
            return await _context.AlarmEvents.ToListAsync();
        }

        public async Task AddAlarmAsync(AlarmEvent alarm)
        {
            await _context.AlarmEvents.AddAsync(alarm);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAlarmAsync(AlarmEvent alarm)
        {
            _context.AlarmEvents.Update(alarm);
            await _context.SaveChangesAsync();
        }
    }
}
