using OTAS.DTO.Post;
using OTAS.DTO.Put;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IService
{
    public interface ILiquidationService
    {
        Task<ServiceResult> LiquidateAvanceVoyage(AvanceVoyage avanceVoyageDB, LiquidationPostDTO avanceVoyageLiquidation, int userId);
        Task<ServiceResult> LiquidateAvanceCaisse(AvanceCaisse avanceCaisseDB, LiquidationPostDTO avanceCaisseLiquidation, int userId);
        Task<ServiceResult> DeleteDraftedLiquidation(Liquidation liquidation);
        Task<ServiceResult> ModifyLiquidation(LiquidationPutDTO liquidation_REQ, Liquidation liquidation_DB);
        Task<ServiceResult> SubmitLiquidation(int liquidationId);
        Task<ServiceResult> DecideOnLiquidation(DecisionOnRequestPostDTO decision, int deciderUserId);

    }
}
