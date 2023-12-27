using OTAS.DTO.Post;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IService
{
    public interface IAvanceVoyageService
    {
        Task<ServiceResult> ModifyAvanceVoyagesForEachCurrency(OrdreMission ordreMission, List<AvanceVoyage> avanceVoyages, List<TripPostDTO> mixedTrips, List<ExpensePostDTO>? mixedExpenses, string action);
        Task<ServiceResult> CreateAvanceVoyageForEachCurrency(OrdreMission ordreMission, List<TripPostDTO> mixedTrips, List<ExpensePostDTO>? mixedExpenses);
        Task<ServiceResult> SubmitAvanceVoyage(int avanceVoyageId);
        Task<ServiceResult> DecideOnAvanceVoyage(DecisionOnRequestPostDTO decision);
    }
}
