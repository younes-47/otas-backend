using OTAS.DTO.Post;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IService
{
    public interface ILiquidationService
    {
        Task<ServiceResult> LiquidateAvanceVoyage(AvanceVoyage avanceVoyageDB, LiquidationPostDTO avanceVoyageLiquidation, int userId);
        Task<ServiceResult> LiquidateAvanceCaisse(AvanceCaisse avanceCaisseDB, LiquidationPostDTO avanceCaisseLiquidation, int userId);
        Task<ServiceResult> DeleteDraftedLiquidation(Liquidation liquidation);
    }
}
