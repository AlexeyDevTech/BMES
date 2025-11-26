using BMES.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BMES.Contracts.Interfaces
{
    public interface IMaterialLotRepository
    {
        Task<IEnumerable<MaterialLot>> GetAllMaterialLotsAsync();
        Task<MaterialLot> GetMaterialLotByIdAsync(int id);
        Task AddMaterialLotAsync(MaterialLot materialLot);
        Task UpdateMaterialLotAsync(MaterialLot materialLot);
        Task DeleteMaterialLotAsync(int id);
    }
}
