using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface ITripRepository
    {
        Task<List<Trip>> GetAvanceVoyageTripsByAvIdAsync(int avId);
        Task<decimal> GetAvanceVoyageTripsActualTotalByAvIdAsync(int avId);
        Task<Trip?> FindTripAsync(int tripId);
        Task<Trip> GetTripAsync(int tripId);
        Task<ServiceResult> AddTripsAsync(List<Trip> trips);
        Task<ServiceResult> AddTripAsync(Trip trip);
        Task<ServiceResult> UpdateTrip(Trip trip);
        Task<ServiceResult> DeleteTrips(List<Trip> trips);
        Task<ServiceResult> DeleteTrip(Trip trip);
        Task<bool> SaveAsync();
    }
}
