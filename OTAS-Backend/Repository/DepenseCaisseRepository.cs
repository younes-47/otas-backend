using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;
using Microsoft.EntityFrameworkCore;
using OTAS.Services;
using Azure.Core;

namespace OTAS.Repository
{
    public class DepenseCaisseRepository : IDepenseCaisseRepository
    {
        //Inject datacontext in the constructor
        private readonly OtasContext _context;
        public DepenseCaisseRepository(OtasContext context)
        {
            _context = context;
        }

        public async Task<DepenseCaisse?> FindDepenseCaisseAsync(int depenseCaisseId)
        {
            return await _context.DepenseCaisses.FindAsync(depenseCaisseId);
        }

        public async Task<ServiceResult> AddDepenseCaisseAsync(DepenseCaisse depenseCaisse)
        {
            ServiceResult result = new();
            await _context.AddAsync(depenseCaisse);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "DepenseCaisse has been added successfully" : "Something went wrong while adding the DepenseCaisse";
            return result;
        }

        public async Task<ServiceResult> UpdateDepenseCaisseAsync(DepenseCaisse depenseCaisse)
        {
            ServiceResult result = new();
            _context.Update(depenseCaisse);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "DepenseCaisse has been updated successfully" : "Something went wrong while updating the DepenseCaisse";
            return result;
        }

        public async Task<DepenseCaisse> GetDepenseCaisseByIdAsync(int depenseCaisseId)
        {
            return await _context.DepenseCaisses.Where(dc => dc.Id == depenseCaisseId).FirstAsync();
        }

        public async Task<List<DepenseCaisse>> GetDepensesCaisseByUserIdAsync(int userId)
        {
            return await _context.DepenseCaisses.Where(dc => dc.UserId == userId).ToListAsync();
        }

        public async Task<List<DepenseCaisse>> GetDepensesCaisseByStatus(int status)
        {
            return await _context.DepenseCaisses.Where(dc => dc.LatestStatus == status).ToListAsync();
        }

        public async Task<bool> SaveAsync()
        {
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}
