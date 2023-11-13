﻿using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface IOrdreMissionRepository
    {
        Task<OrdreMission> GetOrdreMissionByIdAsync(int id);
        Task<OrdreMission?> FindOrdreMissionByIdAsync(int ordreMissionId);
        Task<List<OrdreMission>> GetOrdresMissionByUserIdAsync(int userid);
        Task<List<OrdreMission>> GetOrdresMissionByStatusAsync(int status);
        Task<string?> DecodeStatusAsync(int statusCode);
        Task<bool> AddOrdreMissionAsync(OrdreMission ordreMission);
        Task<ServiceResult> UpdateOrdreMissionStatusAsync(int ordreMissionId, int status);
        Task<bool> SaveAsync();
    }
}