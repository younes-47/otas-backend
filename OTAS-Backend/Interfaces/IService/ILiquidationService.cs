using OTAS.DTO.Post;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IService
{
    public interface ILiquidationService
    {
        Task<ServiceResult> LiquidateAvanceVoyage(List<Trip> tripsDB, List<Expense> expensesDB, AvanceVoyageLiquidationPostDTO avanceVoyageLiquidation, int userId);
    }
}
