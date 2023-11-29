using OTAS.DTO.Get;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface IAvanceCaisseRepository
    {
        Task<AvanceCaisse> GetAvanceCaisseByIdAsync(int idavanceCaisseId);
        Task<AvanceCaisse?> FindAvanceCaisseAsync(int avanceCaisseId);
        Task<List<AvanceCaisse>> GetAvancesCaisseByStatusAsync(int status);
        Task<List<AvanceCaisse>> GetAvancesCaisseByUserIdAsync(int userId);
        Task<ServiceResult> AddAvanceCaisseAsync(AvanceCaisse avanceCaisse);
        Task<ServiceResult> UpdateAvanceCaisseStatusAsync(int avanceCaisseId, int status);
        Task<ServiceResult> UpdateAvanceCaisseAsync(AvanceCaisse avanceCaisse);
        Task<bool> SaveAsync();

    }
}
