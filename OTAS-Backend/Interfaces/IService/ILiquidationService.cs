using OTAS.DTO.Post;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IService
{
    public interface ILiquidationService
    {
        Task<ServiceResult> LiquidateAvanceVoyage(AvanceVoyage avanceVoyageDB, AvanceVoyageLiquidationPostDTO avanceVoyageLiquidation, int userId);
        Task<ServiceResult> LiquidateAvanceCaisse(AvanceCaisse avanceCaisseDB, AvanceCaisseLiquidationPostDTO avanceCaisseLiquidation, int userId);
    }
}
