using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Get;
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
        Task<ServiceResult> DecideOnOrdreMissionWithAvanceVoyage(int ordreMissionId,int deciderUserId,string? deciderComment,int decision);
        Task<ServiceResult> ModifyOrdreMissionWithAvanceVoyage(OrdreMissionPostDTO ordreMission, int currentStatus);
        Task<List<OrdreMissionDTO>> GetOrdreMissionsForDeciderTable(int userId);
    }
}
