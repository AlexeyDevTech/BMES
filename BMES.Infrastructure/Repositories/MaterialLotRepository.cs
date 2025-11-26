using BMES.Contracts.Interfaces;
using BMES.Core.Models;
using BMES.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BMES.Infrastructure.Repositories
{
    public class MaterialLotRepository : IMaterialLotRepository
    {
        private readonly BmesDbContext _context;

        public MaterialLotRepository(BmesDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MaterialLot>> GetAllMaterialLotsAsync()
        {
            return await _context.MaterialLots.ToListAsync();
        }

        public async Task<MaterialLot> GetMaterialLotByIdAsync(int id)
        {
            return (await _context.MaterialLots.FindAsync(id))!;
        }

        public async Task AddMaterialLotAsync(MaterialLot materialLot)
        {
            await _context.MaterialLots.AddAsync(materialLot);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMaterialLotAsync(MaterialLot materialLot)
        {
            _context.MaterialLots.Update(materialLot);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMaterialLotAsync(int id)
        {
            var materialLotToDelete = await _context.MaterialLots.FindAsync(id);
            if (materialLotToDelete != null)
            {
                _context.MaterialLots.Remove(materialLotToDelete);
                await _context.SaveChangesAsync();
            }
        }
    }
}
