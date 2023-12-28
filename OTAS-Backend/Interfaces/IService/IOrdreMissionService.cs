﻿using Microsoft.AspNetCore.Mvc;
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
        Task<ServiceResult> DecideOnOrdreMission(DecisionOnRequestPostDTO decision);
        Task<ServiceResult> ModifyOrdreMissionWithAvanceVoyage(OrdreMissionPutDTO ordreMission);
        Task<List<OrdreMissionDTO>> GetOrdreMissionsForDeciderTable(int userId);
    }
}
