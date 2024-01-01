using Microsoft.EntityFrameworkCore;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface ILiquidationRepository
    {
        Task<Liquidation> GetLiquidationByIdAsync(int id);
        Task<ServiceResult> AddLiquidationAsync(Liquidation liquidation);
        Task<bool> SaveAsync();

    }
}
