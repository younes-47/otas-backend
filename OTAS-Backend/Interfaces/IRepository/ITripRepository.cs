using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface ITripRepository
    {
        ICollection<Trip> GetAvanceVoyageTripsByAvId(int avId);
        decimal GetAvanceVoyageTripsEstimatedTotalByAvId(int avId);
        decimal GetAvanceVoyageTripsActualTotalByAvId(int avId);
        bool AddTrips(ICollection<Trip> trips);
        bool Save();
    }
}
