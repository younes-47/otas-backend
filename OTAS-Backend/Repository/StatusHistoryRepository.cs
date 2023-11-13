using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;

namespace OTAS.Repository
{
    public class StatusHistoryRepository : IStatusHistoryRepository
    {
        private readonly OtasContext _context;
        public StatusHistoryRepository(OtasContext context)
        {
            _context = context;
        }

        //Get Methods
        public ICollection<StatusHistory> GetAvanceCaisseStatusHistory(int acId)
        {
            return _context.StatusHistories.Where(sh => sh.AvanceCaisseId == acId).ToList();
        }

        public ICollection<StatusHistory> GetAvanceVoyageStatusHistory(int avId)
        {
            return _context.StatusHistories.Where(sh => sh.AvanceVoyageId == avId).ToList();
        }

        public ICollection<StatusHistory> GetDepenseCaisseStatusHistory(int dcId)
        {
            return _context.StatusHistories.Where(sh => sh.DepenseCaisseId == dcId).ToList();
        }

        public ICollection<StatusHistory> GetLiquidationStatusHistory(int lqId)
        {
            return _context.StatusHistories.Where(sh => sh.LiquidationId == lqId).ToList();
        }

        public ICollection<StatusHistory> GetOrdreMissionStatusHistory(int omId)
        {
            return _context.StatusHistories.Where(sh => sh.OrdreMissionId == omId).ToList();
        }


        //
        public async Task<bool> AddStatusAsync(StatusHistory status)
        {
            await _context.AddAsync(status);
            return await SaveAsync();
        }

        public async Task<bool> SaveAsync()
        {
            int saved = await _context.SaveChangesAsync();
            return saved > 0;
        }
    }
}
