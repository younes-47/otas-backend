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


        public async Task<decimal> GetAvanceVoyageTripsActualTotalByAvIdAsync(int avId)
        {
            decimal actualTotal = 0;
            List<Trip> trips = await _context.Trips.Where(trip => trip.AvanceVoyageId == avId).ToListAsync();

            if(trips.Count <= 0)
                return 0;
            
            
            foreach (var trip in trips)
            {
                actualTotal += (decimal)trip.ActualFee;
            }

            
            return actualTotal;
        }

        public async Task<List<Trip>> GetAvanceVoyageTripsByAvIdAsync(int avId)
        {
            return await _context.Trips.Where(trip => trip.AvanceVoyageId == avId).ToListAsync();
        }

        public async Task<Trip?> FindTripAsync(int tripId)
        {
            return await _context.Trips.FindAsync(tripId);
        }

        public async Task<Trip> GetTripAsync(int tripId)
        {
            return await _context.Trips.Where(trip => trip.Id == tripId).FirstAsync();
        }

        public async Task<ServiceResult> AddTripsAsync(List<Trip> trips)
        {
            ServiceResult result = new();
            await _context.AddRangeAsync(trips);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "Trips added successfully" : "Something went wrong while saving the trips.";
            return result;
        }

        public async Task<ServiceResult> AddTripAsync(Trip trip)
        {
            ServiceResult result = new();
            await _context.AddAsync(trip);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "Trip added successfully" : "Something went wrong while saving the trip.";
            return result;
        }

        public async Task<ServiceResult> UpdateTrip(Trip trip)
        {
            ServiceResult result = new();
            _context.Update(trip);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "Trip updated successfully" : "Something went wrong while updating a trip.";
            return result;
        }

        public async Task<ServiceResult> DeleteTrips(List<Trip> trips)
        {
            ServiceResult result = new();
            _context.RemoveRange(trips);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "Trips deleted successfully" : "Something went wrong while deleting the trips.";
            return result;

        }

        public async Task<ServiceResult> DeleteTrip(Trip trip)
        {
            ServiceResult result = new();
            _context.Remove(trip);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "Trip deleted successfully" : "Something went wrong while deleting the trip.";
            return result;
        }

        public async Task<bool> SaveAsync()
        {
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

    }
}
