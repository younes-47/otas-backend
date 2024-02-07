using Microsoft.EntityFrameworkCore;
using OTAS.DTO.Get;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface ILiquidationRepository
    {
        Task<Liquidation?> FindLiquidationAsync(int liquidationId);
        Task<ServiceResult> DeleteLiquidationAync(Liquidation liquidation);
        Task<LiquidationViewDTO> GetLiquidationFullDetailsByIdForRequester(int liquidationId);
        Task<Liquidation> GetLiquidationByIdAsync(int id);
        Task<ServiceResult> AddLiquidationAsync(Liquidation liquidation);
        Task<List<LiquidationTableDTO>> GetLiquidationsTableForRequster(int userId);
        Task<List<LiquidationTableDTO>> GetLiquidationsTableForDecider(int userId);
        Task<List<RequestToLiquidate>> GetRequestsToLiquidatesByTypeAsync(string type, int userId);
        Task<bool> SaveAsync();

    }
}
