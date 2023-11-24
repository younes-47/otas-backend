using Microsoft.EntityFrameworkCore;
using OTAS.Data;
using OTAS.Interfaces.IRepository;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Repository
{
    public class TestingRepository : ITestingRepository
    {
        private readonly OtasContext _context;

        public TestingRepository(OtasContext context)
        {
            _context = context;
        }


        public async Task<List<ActualRequester>> GetAllActualRequestersAsync()
        {
            return await _context.ActualRequesters.ToListAsync();
        }

        public async Task<List<AvanceCaisse>> GetAllAvanceCaissesAsync()
        {
            return await _context.AvanceCaisses.ToListAsync();
        }

        public async Task<List<AvanceVoyage>> GetAllAvanceVoyagesAsync()
        {
            return await _context.AvanceVoyages.ToListAsync();
        }

        public async Task<List<Expense>> GetAllExpensesAsync()
        {
            return await _context.Expenses.ToListAsync();
        }

        public async Task<List<OrdreMission>> GetAllOrdreMissionsAsync()
        {
            return await _context.OrdreMissions.ToListAsync();
        }

        public async Task<List<StatusHistory>> GetAllStatusHistoriesAsync()
        {
            return await _context.StatusHistories.ToListAsync();
        }

        public async Task<List<Trip>> GetAllTripsAsync()
        {
            return await _context.Trips.ToListAsync();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.OrderBy(user => user.Id).ToListAsync();
        }

        public async Task<ServiceResult> AddUserAsync(User user)
        {
            ServiceResult result = new();
            await _context.AddAsync(user);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "User added Successfully." : "Something went wrong while adding the user";
            return result;
        }

        public async Task<bool> SaveAsync()
        {
            int saved = await _context.SaveChangesAsync();
            return saved > 0;
        }


    }
}
