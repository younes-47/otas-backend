using OTAS.DTO.Get;
using OTAS.Models;
using OTAS.Services;
using System.Threading.Tasks;

namespace OTAS.Interfaces.IRepository
{
    public interface IDepenseCaisseRepository
    {
        Task<DepenseCaisse?> FindDepenseCaisseAsync(int depenseCaisseId);
        Task<DepenseCaisse> GetDepenseCaisseByIdAsync(int id);
        Task<DepenseCaisseViewDTO> GetDepenseCaisseFullDetailsById(int depenseCaisseId);
        Task<DepenseCaisseDeciderViewDTO> GetDepenseCaisseFullDetailsByIdForDecider(int avanceCaisseId);
        Task<ServiceResult> UpdateDepenseCaisseAsync(DepenseCaisse depenseCaisse);
        Task<ServiceResult> AddDepenseCaisseAsync(DepenseCaisse depenseCaisse);
        Task<ServiceResult> UpdateDepenseCaisseStatusAsync(int depesneCaisseId, int status);
        Task<ServiceResult> DeleteDepenseCaisseAync(DepenseCaisse depenseCaisse);
        Task<List<DepenseCaisse>> GetDepensesCaisseByStatus(int status);
        Task<List<DepenseCaisseDTO>> GetDepensesCaisseByUserIdAsync(int userId);
        Task<List<DepenseCaisseDeciderTableDTO>> GetDepenseCaissesForDeciderTable(int deciderUserId);
        Task<int> GetDepenseCaisseNextDeciderUserId(string currentlevel, bool? isReturnedToFMByTR = false, bool? isReturnedToTRbyFM = false);
        Task<decimal[]> GetRequesterAllTimeRequestedAmountsByUserIdAsync(int userId);
        Task<decimal[]> GetDeciderAllTimeDecidedUponAmountsByUserIdAsync(int userId);
        Task<DateTime?> GetTheLatestCreatedRequestByUserIdAsync(int userId);
        Task<bool> SaveAsync();
    }
}
