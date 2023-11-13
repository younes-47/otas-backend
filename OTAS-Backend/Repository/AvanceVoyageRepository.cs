using OTAS.Data;
using OTAS.Models;
using Microsoft.EntityFrameworkCore;
using OTAS.Interfaces.IRepository;
using OTAS.Services;

namespace OTAS.Repository
{
    public class AvanceVoyageRepository : IAvanceVoyageRepository
    {
        private readonly OtasContext _context;
        public AvanceVoyageRepository(OtasContext context)
        {
            _context = context;
        }

        

        public async Task<List<AvanceVoyage>> GetAvancesVoyageByStatusAsync(int status)
        {
            return await _context.AvanceVoyages.Where(av => av.LatestStatus == status).ToListAsync();
        }

        public async Task<AvanceVoyage> GetAvanceVoyageByIdAsync(int id)
        {
            return await _context.AvanceVoyages.Where(av => av.Id == id).FirstAsync();
        }

        public async Task<AvanceVoyage?> FindAvanceVoyageByIdAsync(int avanceVoyageId)
        {
            return await _context.AvanceVoyages.FindAsync(avanceVoyageId);
        }
        public async Task<List<AvanceVoyage>> GetAvancesVoyageByOrdreMissionIdAsync(int ordreMissionId)
        {
            return await _context.AvanceVoyages.Where(av => av.OrdreMissionId == ordreMissionId).ToListAsync();
        }

        /* ??????????????????????????*/
        public async Task<List<AvanceVoyage>> GetAvancesVoyageByRequesterUserIdAsync(int requesterUserId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AddAvanceVoyageAsync(AvanceVoyage voyage)
        {
            await _context.AddAsync(voyage);
            return await SaveAsync();
        }

        public async Task <ServiceResult> UpdateAvanceVoyageStatusAsync(int avanceVoyageId, int status)
        {
                ServiceResult result = new();

                AvanceVoyage updatedAvanceVoyage  = await GetAvanceVoyageByIdAsync(avanceVoyageId);
                updatedAvanceVoyage.LatestStatus = status;
                _context.AvanceVoyages.Update(updatedAvanceVoyage);

                result.Success = await SaveAsync();
                if (result.Success)
                {
                    result.SuccessMessage = "Updated Successfully";
                }
                else
                {
                    result.ErrorMessage = "Something went wrong while updating";
                }

           return result;

        }

        public async Task<bool> SaveAsync()
        {
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }
    }
}
