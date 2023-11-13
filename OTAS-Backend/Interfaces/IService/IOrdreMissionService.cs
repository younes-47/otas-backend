using OTAS.DTO.Post;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IService
{
    public interface IOrdreMissionService
    {
        Task<ServiceResult> CreateAvanceVoyageForEachCurrency(OrdreMission mappedOM, List<TripPostDTO> mixedTrips, List<ExpensePostDTO> mixedExpenses);
        Task<ServiceResult> CreateOrdreMissionWithAvanceVoyageAsDraft(OrdreMissionPostDTO ordreMissionPostDTO);
        Task<ServiceResult> SubmitOrdreMissionWithAvanceVoyage(int ordreMissionId);

    }
}
