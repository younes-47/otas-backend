using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;
using OTAS.Services;
using Microsoft.EntityFrameworkCore;
using OTAS.DTO.Get;

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
        public async Task<List<StatusHistory>> GetAvanceCaisseStatusHistory(int acId)
        {
            return await _context.StatusHistories.Where(sh => sh.AvanceCaisseId == acId).ToListAsync();
        }

        public async Task<List<StatusHistory>> GetAvanceVoyageStatusHistory(int avId)
        {
            return await _context.StatusHistories.Where(sh => sh.AvanceVoyageId == avId).ToListAsync();
        }

        public async Task<List<StatusHistory>> GetDepenseCaisseStatusHistory(int dcId)
        {
            return await _context.StatusHistories.Where(sh => sh.DepenseCaisseId == dcId).ToListAsync();
        }

        public async Task<List<StatusHistory>> GetLiquidationStatusHistory(int lqId)
        {
            return await _context.StatusHistories.Where(sh => sh.LiquidationId == lqId).ToListAsync();
        }

        public async Task<List<StatusHistory>> GetOrdreMissionStatusHistory(int omId)
        {
            return await _context.StatusHistories.Where(sh => sh.OrdreMissionId == omId).ToListAsync();
        }

        public async Task<StatusHistory> GetLatestAvanceCaisseStatusHistroyById(int acId)
        {
            return await _context.StatusHistories.Where(sh => sh.AvanceCaisseId == acId).FirstAsync();
        }

        public async Task<StatusHistory> GetLatestAvanceVoyageStatusHistroyById(int avId)
        {
            return await _context.StatusHistories.Where(sh => sh.AvanceVoyageId == avId).FirstAsync();
        }

        public async Task<StatusHistory> GetLatestDepenseCaisseStatusHistroyById(int dcId)
        {
            return await _context.StatusHistories.Where(sh => sh.DepenseCaisseId == dcId).FirstAsync();
        }

        public async Task<ServiceResult> UpdateStatusHistoryTotal(int requestId, string requestType, decimal total)
        {
            ServiceResult result = new() { Success = false, Message = "Switch statement has not been executed while updating the SH" };
            switch (requestType) 
            {
                case "AV":
                    var SH_AV = await GetLatestAvanceVoyageStatusHistroyById(requestId);
                    SH_AV.Total = total;
                    _context.StatusHistories.Update(SH_AV);
                    result.Success = await SaveAsync();
                    break;
                case "AC":
                    var SH_AC = await GetLatestAvanceCaisseStatusHistroyById(requestId);
                    SH_AC.Total = total;
                    _context.StatusHistories.Update(SH_AC);
                    result.Success = await SaveAsync();
                    break;
                case "DC":
                    var SH_DC = await GetLatestDepenseCaisseStatusHistroyById(requestId);
                    SH_DC.Total = total;
                    _context.StatusHistories.Update(SH_DC);
                    result.Success = await SaveAsync();
                    break;
                default:
                    result.Success = false;
                    result.Message = "Request Type is invalid (to update SH Total)";
                    break;
            }
            return result;
        }

        public async Task<ServiceResult> DeleteStatusHistories(List<StatusHistory> statusHistories)
        {
            ServiceResult result = new();
            _context.RemoveRange(statusHistories);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "StatusHistories deleted successfully" : "Something went wrong while deleting the StatusHistories.";
            return result;
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
