using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;
using OTAS.Services;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using OTAS.DTO.Get;
using AutoMapper;

namespace OTAS.Repository
{
    public class AvanceCaisseRepository : IAvanceCaisseRepository
    {
        private readonly OtasContext _context;

        public AvanceCaisseRepository(OtasContext context)
        {
            _context = context;
        }

        public async Task<ServiceResult> AddAvanceCaisseAsync(AvanceCaisse avanceCaisse)
        {
            ServiceResult result = new();
            await _context.AddAsync(avanceCaisse);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "\"AvanceCaisse\" added successfully" : "Something went wrong while adding \"AvanceCaisse\"";
            return result;
        }

        public async Task<AvanceCaisse?> FindAvanceCaisseAsync(int avanceCaisseId)
        {
            return await _context.AvanceCaisses.FindAsync(avanceCaisseId);
        }

        public async Task<AvanceCaisse> GetAvanceCaisseByIdAsync(int avanceCaisseId)
        {
            return await _context.AvanceCaisses.Where(ac => ac.Id == avanceCaisseId).FirstAsync();
        }

        public async Task<List<AvanceCaisse>> GetAvancesCaisseByStatusAsync(int status)
        {
            return await _context.AvanceCaisses.Where(ac => ac.LatestStatus == status).ToListAsync();
        }

        public async Task<List<AvanceCaisse>> GetAvancesCaisseByUserIdAsync(int userId)
        {
            return await _context.AvanceCaisses.Where(ac => ac.UserId == userId).ToListAsync();
        }

        public async Task<bool> SaveAsync()
        {
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

        public async Task<ServiceResult> UpdateAvanceCaisseAsync(AvanceCaisse avanceCaisse)
        {
            ServiceResult result = new();
            _context.Update(avanceCaisse);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "\"AvanceCaisse\" updated successfully" : "Something went wrong while updating \"AvanceCaisse\"";
            return result;
        }

        public async Task<ServiceResult> UpdateAvanceCaisseStatusAsync(int avanceCaisseId, int status)
        {
            ServiceResult result = new();

            AvanceCaisse updatedAvanceCaisse = await GetAvanceCaisseByIdAsync(avanceCaisseId);
            updatedAvanceCaisse.LatestStatus = status;
            _context.AvanceCaisses.Update(updatedAvanceCaisse);

            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "\"AvanceCaisse\" Status Updated Successfully" : "Something went wrong while updating \"AvanceCaisse\" Status";

            return result;
        }
    }
}
