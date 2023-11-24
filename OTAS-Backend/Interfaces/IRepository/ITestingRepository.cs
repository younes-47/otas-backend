using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface ITestingRepository
    {
        Task<List<OrdreMission>> GetAllOrdreMissionsAsync();
        Task<List<AvanceCaisse>> GetAllAvanceCaissesAsync();
        Task<List<AvanceVoyage>> GetAllAvanceVoyagesAsync();
        Task<List<ActualRequester>> GetAllActualRequestersAsync();
        Task<List<Expense>> GetAllExpensesAsync();
        Task<List<Trip>> GetAllTripsAsync();
        Task<List<StatusHistory>> GetAllStatusHistoriesAsync();
        Task<List<User>> GetAllUsersAsync();
        Task<ServiceResult> AddUserAsync(User user);
        Task<bool> SaveAsync();
    }
}
