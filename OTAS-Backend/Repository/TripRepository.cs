using OTAS.Data;
using OTAS.Models;
using Microsoft.EntityFrameworkCore;
using OTAS.Interfaces.IRepository;
using OTAS.Services;

namespace OTAS.Repository
{
    public class TripRepository : ITripRepository
    {
        private readonly OtasContext _context;
        public TripRepository(OtasContext context)
        {
            _context = context;
        }


        public decimal GetAvanceVoyageTripsEstimatedTotalByAvId(int avId)
        {
            decimal estimatedTotal = 0;
            ICollection<Trip> trips = _context.Trips.Where(trip => trip.AvanceVoyageId == avId).ToList();

            if (trips.Count <= 0)
                return 0;


            foreach (var trip in trips)
            {
                //estimatedTotal += trip.EstimatedFee;
            }


            return estimatedTotal;
        }

        public decimal GetAvanceVoyageTripsActualTotalByAvId(int avId)
        {
            decimal actualTotal = 0;
            ICollection<Trip> trips = _context.Trips.Where(trip => trip.AvanceVoyageId == avId).ToList();

            if(trips.Count <= 0)
                return 0;
            
            
            foreach (var trip in trips)
            {
                actualTotal += (decimal)trip.ActualFee;
            }

            
            return actualTotal;
        }

        public ICollection<Trip> GetAvanceVoyageTripsByAvId(int avId)
        {
            return _context.Trips.Where(trip => trip.AvanceVoyageId == avId).ToList();
        }

        public async Task<ServiceResult> AddTripsAsync(ICollection<Trip> trips)
        {
            ServiceResult result = new();
            await _context.AddRangeAsync(trips);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "Trips added successfully" : "Something went wrong while saving the trips.";
            return result;
        }

        public async Task<bool> SaveAsync()
        {
            var saved = await _context.SaveChangesAsync();
            return saved > 0 ? true : false;
        }
    }
}
