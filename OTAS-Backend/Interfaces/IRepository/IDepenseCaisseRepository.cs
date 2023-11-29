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
        Task<List<DepenseCaisse>> GetDepensesCaisseByStatus(int status);
        Task<List<DepenseCaisse>> GetDepensesCaisseByUserIdAsync(int userId);
        Task<bool> SaveAsync();
    }
}
