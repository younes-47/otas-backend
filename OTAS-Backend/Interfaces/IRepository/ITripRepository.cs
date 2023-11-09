using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface ITripRepository
    {
        public ICollection<Trip> GetAvanceVoyageTripsByAvId(int avId);
        public decimal GetAvanceVoyageTripsEstimatedTotalByAvId(int avId);
        public decimal GetAvanceVoyageTripsActualTotalByAvId(int avId);
        bool AddTrips(ICollection<Trip> trips);
        bool Save();
    }
}
