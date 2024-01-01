using Microsoft.EntityFrameworkCore;
using OTAS.DTO.Get;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface ILiquidationRepository
    {
        Task<Liquidation> GetLiquidationByIdAsync(int id);
        Task<ServiceResult> AddLiquidationAsync(Liquidation liquidation);
        Task<List<LiquidationTableDTO>> GetLiquidationsTableForRequster(int userId);
        Task<bool> SaveAsync();

    }
}
