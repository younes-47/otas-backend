using OTAS.DTO.Get;
using OTAS.Models;
using OTAS.Repository;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface IOrdreMissionRepository
    {
        Task<List<OrdreMissionDTO>> GetOrdreMissionsForDecider(int deciderUserId);
        Task<OrdreMission> GetOrdreMissionByIdAsync(int id);
        Task<OrdreMissionViewDTO> GetOrdreMissionFullDetailsById(int ordreMissionId);
        Task<OrdreMission?> FindOrdreMissionByIdAsync(int ordreMissionId);
        Task<List<OrdreMissionDTO>?> GetOrdresMissionByUserIdAsync(int userid);
        Task<int> GetOrdreMissionNextDeciderUserId(string currentlevel, bool? isLongerThanOneDay = false);
        Task<List<OrdreMission>> GetOrdresMissionByStatusAsync(int status);
        Task<string?> DecodeStatusAsync(int statusCode);
        Task<ServiceResult> AddOrdreMissionAsync(OrdreMission ordreMission);
        Task<ServiceResult> UpdateOrdreMissionStatusAsync(int ordreMissionId, int status, int nextDeciderUserId);
        Task<ServiceResult> UpdateOrdreMission(OrdreMission ordreMission);
        Task<ServiceResult> DeleteOrdreMissionAsync(OrdreMission ordreMission);
        Task<bool> SaveAsync();
    }
}
