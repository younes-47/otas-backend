using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface ITestingRepository
    {
        Task<List<OrdreMission>> GetAllOrdreMissions();
        Task<List<AvanceVoyage>> GetAllAvanceVoyages();
        Task<List<ActualRequester>> GetAllActualRequesters();
        Task<List<Expense>> GetAllExpenses();
        Task<List<Trip>> GetAllTrips();
        Task<List<StatusHistory>> GetAllStatusHistories();
    }
}
