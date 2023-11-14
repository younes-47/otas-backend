using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface ITripRepository
    {
        ICollection<Trip> GetAvanceVoyageTripsByAvId(int avId);
        decimal GetAvanceVoyageTripsEstimatedTotalByAvId(int avId);
        decimal GetAvanceVoyageTripsActualTotalByAvId(int avId);
        Task<ServiceResult> AddTripsAsync(ICollection<Trip> trips);
        Task<bool> SaveAsync();
    }
}
