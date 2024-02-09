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
        Task<List<LiquidationDeciderTableDTO>> GetLiquidationsTableForDecider(int deciderUserId);
        Task<List<RequestToLiquidate>> GetRequestsToLiquidatesByTypeAsync(string type, int userId);
        Task<LiquidationDeciderViewDTO> GetLiquidationFullDetailsByIdForDecider(int liquidationId);
        Task<int> GetLiquidationNextDeciderUserId(string currentlevel, bool? isReturnedToFMByTR = false, bool? isReturnedToTRbyFM = false);
        Task<ServiceResult> UpdateLiquidationAsync(Liquidation liquidation);
        Task<bool> SaveAsync();

    }
}
