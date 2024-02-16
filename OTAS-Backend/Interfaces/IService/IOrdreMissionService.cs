using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.DTO.Put;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IService
{
    public interface IOrdreMissionService
    {
        Task<ServiceResult> CreateOrdreMissionWithAvanceVoyageAsDraft(OrdreMissionPostDTO ordreMissionPostDTO, int userId);
        Task<ServiceResult> SubmitOrdreMissionWithAvanceVoyage(int ordreMissionId);
        Task<ServiceResult> DecideOnOrdreMission(DecisionOnRequestPostDTO decision, int deciderUserId);
        Task<ServiceResult> ApproveOrdreMissionWithAvanceVoyage(int ordreMissionId, int deciderUserId);
        Task<ServiceResult> ModifyOrdreMissionWithAvanceVoyage(OrdreMissionPutDTO ordreMission);
        Task<ServiceResult> DeleteDraftedOrdreMissionWithAvanceVoyages(OrdreMission ordreMission);
    }
}
