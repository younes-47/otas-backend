using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IService
{
    public interface IOrdreMissionService
    {
        Task<ServiceResult> CreateOrdreMissionWithAvanceVoyageAsDraft(OrdreMissionPostDTO ordreMissionPostDTO);
        Task<ServiceResult> SubmitOrdreMissionWithAvanceVoyage(int ordreMissionId);
        Task<ServiceResult> DecideOnOrdreMission(DecisionOnRequestPostDTO decision);
        Task<ServiceResult> ModifyOrdreMissionWithAvanceVoyage(OrdreMissionPostDTO ordreMission, string action);
        Task<List<OrdreMissionDTO>> GetOrdreMissionsForDeciderTable(int userId);
    }
}
