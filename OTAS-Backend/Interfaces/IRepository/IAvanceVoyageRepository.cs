using OTAS.DTO.Get;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface IAvanceVoyageRepository
    {
        //Get Methods
        Task<AvanceVoyage> GetAvanceVoyageByIdAsync(int id);
        Task<AvanceVoyage?> FindAvanceVoyageByIdAsync(int avanceVoyageId);
        Task<List<AvanceVoyage>> GetAvancesVoyageByUserIdAsync(int userId);
        Task<List<AvanceVoyage>> GetAvancesVoyageByStatusAsync(int status);
        Task<List<AvanceVoyage>> GetAvancesVoyageByOrdreMissionIdAsync(int ordreMissionId);

        // This method should probably be in a service rather than the repo (like AC) but whatever
        Task<List<AvanceVoyageTableDTO>> GetAvanceVoyagesForDeciderTable(int deciderRole);

        //Post Methods
        Task<ServiceResult> AddAvanceVoyageAsync(AvanceVoyage voyage);
        Task<bool> SaveAsync();

        //Put Methods
        Task<ServiceResult> UpdateAvanceVoyageStatusAsync(int avanceVoyageId, int status);
        Task<ServiceResult> UpdateAvanceVoyageAsync(AvanceVoyage avanceVoyage);
    }
}
