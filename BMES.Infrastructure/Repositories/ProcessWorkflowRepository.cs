using BMES.Contracts.Interfaces;
using BMES.Core.Models;
using BMES.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace BMES.Infrastructure.Repositories
{
    public class ProcessWorkflowRepository : IProcessWorkflowRepository
    {
        private readonly BmesDbContext _context;

        public ProcessWorkflowRepository(BmesDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProcessWorkflow>> GetAllProcessWorkflowsAsync()
        {
            return await _context.ProcessWorkflows
                                 .Include(pw => pw.Steps)
                                 .OrderBy(pw => pw.Name)
                                 .ToListAsync();
        }

        public async Task<ProcessWorkflow> GetProcessWorkflowByIdAsync(int id)
        {
            return (await _context.ProcessWorkflows
                                 .Include(pw => pw.Steps)
                                 .FirstOrDefaultAsync(pw => pw.Id == id))!;
        }

        public async Task AddProcessWorkflowAsync(ProcessWorkflow workflow)
        {
            await _context.ProcessWorkflows.AddAsync(workflow);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProcessWorkflowAsync(ProcessWorkflow workflow)
        {
            _context.ProcessWorkflows.Update(workflow);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProcessWorkflowAsync(int id)
        {
            var workflowToDelete = await _context.ProcessWorkflows.FindAsync(id);
            if (workflowToDelete != null)
            {
                _context.ProcessWorkflows.Remove(workflowToDelete);
                await _context.SaveChangesAsync();
            }
        }
    }
}
