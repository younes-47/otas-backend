using OTAS.Data;
using OTAS.Models;
using Microsoft.EntityFrameworkCore;
using OTAS.Interfaces.IRepository;

namespace OTAS.Repository
{
    public class AvanceVoyageRepository : IAvanceVoyageRepository
    {
        private readonly OtasContext _Context;
        public AvanceVoyageRepository(OtasContext context)
        {
            _Context = context;
        }

        

        public async Task<List<AvanceVoyage>> GetAvancesVoyageByStatus(int status)
        {
            return await _Context.AvanceVoyages.Where(av => av.LatestStatus == status).ToListAsync();
        }

        public async Task<AvanceVoyage> GetAvanceVoyageById(int id)
        {
            return await _Context.AvanceVoyages.Where(av => av.Id == id).FirstOrDefaultAsync();
        }
        public async Task<List<AvanceVoyage>> GetAvancesVoyageByOrdreMissionId(int ordreMissionId)
        {
            return await _Context.AvanceVoyages.Where(av => av.OrdreMissionId == ordreMissionId).ToListAsync();
        }

        /* ??????????????????????????*/
        public async Task<List<AvanceVoyage>> GetAvancesVoyageByRequesterUserId(int requesterUserId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AddAvanceVoyageAsync(AvanceVoyage voyage)
        {
            await _Context.AddAsync(voyage);
            return await SaveAsync();
        }

        public async Task<bool> SaveAsync()
        {
            var saved = await _Context.SaveChangesAsync();
            return saved > 0;
        }
    }
}
