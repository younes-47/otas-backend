using OTAS.DTO.Get;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface IDepenseCaisseRepository
    {
        Task<DepenseCaisse?> FindDepenseCaisseAsync(int depenseCaisseId);
        Task<DepenseCaisse> GetDepenseCaisseByIdAsync(int id);
        Task<ServiceResult> UpdateDepenseCaisseAsync(DepenseCaisse depenseCaisse);
        Task<ServiceResult> AddDepenseCaisseAsync(DepenseCaisse depenseCaisse);
        Task<ServiceResult> UpdateDepenseCaisseStatusAsync(int depesneCaisseId, int status);
        Task<List<DepenseCaisse>> GetDepensesCaisseByStatus(int status);
        Task<List<DepenseCaisseDTO>> GetDepensesCaisseByUserIdAsync(int userId);
        Task<bool> SaveAsync();
    }
}
