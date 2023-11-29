using OTAS.DTO.Get;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface IOrdreMissionRepository
    {
        Task<OrdreMission> GetOrdreMissionByIdAsync(int id);
        Task<OrdreMissionFullDetailsDTO> GetOrdreMissionFullDetailsById(int ordreMissionId);
        Task<OrdreMission?> FindOrdreMissionByIdAsync(int ordreMissionId);
        Task<List<OrdreMission>?> GetOrdresMissionByUserIdAsync(int userid);
        Task<List<OrdreMission>> GetOrdresMissionByStatusAsync(int status);
        Task<string?> DecodeStatusAsync(int statusCode);
        Task<ServiceResult> AddOrdreMissionAsync(OrdreMission ordreMission);
        Task<ServiceResult> UpdateOrdreMissionStatusAsync(int ordreMissionId, int status);
        Task<ServiceResult> UpdateOrdreMission(OrdreMission ordreMission);
        Task<bool> SaveAsync();
    }
}
