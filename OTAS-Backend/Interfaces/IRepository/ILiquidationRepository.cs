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

        Task<int> GetRequestsToLiquidateCountForDecider();
        Task<int> GetRequestsToLiquidateCountForRequester(int userId);
        Task<decimal> GetRequestsToLiquidateTotalByCurrencyForDecider(string currency);
        Task<decimal> GetRequestsToLiquidateTotalByCurrencyForRequester(string currency, int userId);
        Task<LiquidationAvanceCaisseDocumentDetailsDTO> GetAvanceCaisseLiquidationDocumentDetailsByIdAsync(int liquidationId);
        Task<LiquidationAvanceVoyageDocumentDetailsDTO> GetAvanceVoyageLiquidationDocumentDetailsByIdAsync(int liquidationId);
        Task<int> GetOngoingLiquidationsCountForDecider();
        Task<int> GetOngoingLiquidationsCountForRequester(int userId);
        Task<decimal> GetOngoingLiquidationsTotalByCurrencyForDecider(string currency);
        Task<decimal> GetOngoingLiquidationsTotalByCurrencyForRequester(string currency, int userId);

        Task<int> GetFinalizedLiquidationsCountForDecider();
        Task<int> GetFinalizedLiquidationsCountForRequester(int userId);

        Task<decimal> GetFinalizedLiquidationsTotalByCurrencyForDecider(string currency);
        Task<decimal> GetFinalizedLiquidationsTotalByCurrencyForRequester(string currency, int userId);

        Task<int> GetLiquidationNextDeciderUserId(string currentlevel, bool? isReturnedToFMByTR = false, bool? isReturnedToTRbyFM = false);
        Task<ServiceResult> UpdateLiquidationAsync(Liquidation liquidation);
        Task<bool> SaveAsync();

    }
}
