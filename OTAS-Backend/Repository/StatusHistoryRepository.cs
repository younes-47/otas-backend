using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;
using OTAS.Services;

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
        public async Task<ServiceResult> AddStatusAsync(StatusHistory status)
        {
            ServiceResult result = new();
            await _context.AddAsync(status);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "StatusHistory Added Successfully" : "Something went wrong while registring StatusHistory";

            return result;
        }

        public async Task<bool> SaveAsync()
        {
            int saved = await _context.SaveChangesAsync();
            return saved > 0;
        }
    }
}
