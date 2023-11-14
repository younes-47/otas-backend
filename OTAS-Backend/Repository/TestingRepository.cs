using Microsoft.EntityFrameworkCore;
using OTAS.Data;
using OTAS.Interfaces.IRepository;
using OTAS.Models;

namespace OTAS.Repository
{
    public class TestingRepository : ITestingRepository
    {
        private readonly OtasContext _context;

        public TestingRepository(OtasContext context)
        {
            _context = context;
        }


        public async Task<List<ActualRequester>> GetAllActualRequesters()
        {
            return await _context.ActualRequesters.ToListAsync();
        }

        public async Task<List<AvanceVoyage>> GetAllAvanceVoyages()
        {
            return await _context.AvanceVoyages.ToListAsync();
        }

        public async Task<List<Expense>> GetAllExpenses()
        {
            return await _context.Expenses.ToListAsync();
        }

        public async Task<List<OrdreMission>> GetAllOrdreMissions()
        {
            return await _context.OrdreMissions.ToListAsync();
        }

        public async Task<List<StatusHistory>> GetAllStatusHistories()
        {
            return await _context.StatusHistories.ToListAsync();
        }

        public async Task<List<Trip>> GetAllTrips()
        {
            return await _context.Trips.ToListAsync();
        }

    }
}
